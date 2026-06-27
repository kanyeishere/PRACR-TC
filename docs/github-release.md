# 使用 GitHub Actions 自动构建发布 PromeRotation ACR

> 本文是一份通用教程，基于 Wotou-TC（一个 BRD ACR，台服 API12 版）的实际经验总结。如果你在开发自己的 PromeRotation ACR，可以照着这个流程搭建自动发布流水线。

---

## 整体思路

这套自动发布方案的核心是：**推送 Git tag → CI 自动编译 → 打包为插件 zip → 生成 PromeRotation 远程 ACR 清单 → 发布到 GitHub Release**。

流程中涉及的核心环节：

1. 编译时引用 DLL 的处理
2. GitHub Actions 工作流的编排
3. 版本号同步（代码 Metadata vs. 发布清单）
4. repo.json 清单生成

下面逐一说明。

---

## 一、项目结构准备

一个典型的 PromeRotation ACR 仓库结构如下：

```
你的ACR仓库/
├── .github/
│   └── workflows/
│       └── release.yml        # CI/CD 工作流
├── 你的职业目录/
│   └── YourRotation.cs        # 含 [RotationMetadata] 的主文件
├── lib/                       # CI 编译引用 DLL
│   ├── Dalamud.dll
│   ├── PromeRotation.dll
│   └── ...
├── docs/                      # （可选）文档
├── YourProject.csproj
└── README.md
```

**关键点**：`lib/` 目录存放编译所需的引用 DLL，因为 GitHub Actions Runner 不能访问你本机上的 Dalamud 和 PromeRotation 路径。

---

## 二、csproj 配置

在 `.csproj` 中，需要做几件事：

### 1. 引用 DLL 用 HintPath 指定，不复制到输出

```xml
<ItemGroup>
    <Reference Include="Dalamud">
        <HintPath>$(DalamudReferenceRoot)\Dalamud.dll</HintPath>
        <Private>false</Private>
    </Reference>
    <Reference Include="PromeRotation">
        <HintPath>$(PromeRotationReferenceRoot)\PromeRotation.dll</HintPath>
        <Private>false</Private>
    </Reference>
    <!-- 其他 DLL 同理 -->
</ItemGroup>
```

`<Private>false</Private>` 确保这些 DLL 不会被打包进最终的 `latest.zip`。

### 2. 暴露路径变量，方便覆盖

```xml
<PropertyGroup>
    <DalamudReferenceRoot Condition="'$(DalamudReferenceRoot)' == ''">
        C:\Users\<你的用户名>\AppData\Roaming\XIVLauncherCN\addon\Hooks\dev
    </DalamudReferenceRoot>
    <PromeRotationReferenceRoot Condition="'$(PromeRotationReferenceRoot)' == ''">
        C:\Users\<你的用户名>\AppData\Roaming\XIVLauncherCN\installedPlugins\PromeRotation\1.0.0.0
    </PromeRotationReferenceRoot>
</PropertyGroup>
```

这样本地编译和 CI 编译都可以通过 `-p:属性名=值` 来覆盖路径。CI 中会传入 `lib/` 目录的绝对路径。

### 3. 本地自动同步引用 DLL 到 lib/

如果你希望本地 `dotnet build` 时自动将引用 DLL 复制到 `lib/` 供 CI 使用，可以加一个 MSBuild Target，这对于后边实现自动发布很关键：

```xml
<Target Name="SyncReferenceDllsToLib" BeforeTargets="BeforeBuild"
        Condition="'$(GITHUB_ACTIONS)' != 'true'">
    <MakeDir Directories="$(MSBuildProjectDirectory)\lib" />
    <ItemGroup>
        <ReferenceDllsForCi Include="$(DalamudReferenceRoot)\Dalamud.dll" />
        <ReferenceDllsForCi Include="$(PromeRotationReferenceRoot)\PromeRotation.dll" />
        <!-- 其他 DLL -->
    </ItemGroup>
    <Copy SourceFiles="@(ReferenceDllsForCi)"
          DestinationFolder="$(MSBuildProjectDirectory)\lib"
          SkipUnchangedFiles="true"
          Condition="Exists('%(ReferenceDllsForCi.Identity)')" />
</Target>
```

注意 `Condition="'$(GITHUB_ACTIONS)' != 'true'"`：只在本地编译时执行，CI 里跳过。

---

## 三、lib/ — CI 编译引用 DLL

GitHub Actions Runner 是一个干净的 Windows 环境，没有 Dalamud、没有 PromeRotation。所以你需要把编译时引用的 DLL 提交到仓库。

### 需要哪些 DLL

| DLL | 来源 |
|---|---|
| `Dalamud.dll` | XIVLauncher 开发目录（`addon/Hooks/dev/`） |
| `Dalamud.Bindings.ImGui.dll` | XIVLauncher 开发目录 |
| `FFXIVClientStructs.dll` | XIVLauncher 开发目录 |
| `Lumina.dll` | XIVLauncher 开发目录 |
| `Lumina.Excel.dll` | XIVLauncher 开发目录 |
| `ECommons.dll` | PromeRotation 目录 |
| `PromeRotation.dll` | PromeRotation 目录 |

如果你们项目依赖了其他 DLL，也需要一并放入。

### 设置步骤

```powershell
# 1. 本地先编译一次（如果有 SyncReferenceDllsToLib Target 会自动复制）
dotnet build YourProject.csproj

# 2. 提交 lib/
git add lib/
git commit -m "Add reference DLLs for CI build"
git push
```

建议在 `lib/` 下放一个 `README.md` 说明这些 DLL 的来源和用途。

---

## 四、GitHub Actions 工作流

这是整套流程的核心。完整的 workflow 文件放在 `.github/workflows/release.yml`。
你可以直接复制使用

### 触发方式

```yaml
on:
  workflow_dispatch:
    inputs:
      version:
        description: "Release version, for example 1.5.2.2"
        required: true
        default: "1.0.0.0"
  push:
    tags:
      - "v*"
```

触发命令：
- **推送 tag**（如 `git tag v1.5.2.2 && git push origin v1.5.2.2`）

### 工作流总览

一个完整的发布工作流包含以下步骤：

| # | 步骤 | 说明 |
|---|---|---|
| 1 | Checkout | 检出仓库代码 |
| 2 | 解析版本号 | 从 tag（`v1.5.2.2`）中提取版本号，生成 `v` 前缀的 tag 名 |
| 3 | 安装 .NET SDK | 安装项目所需的 .NET 版本 |
| 4 | 检查引用 DLL | 确保 `lib/` 中的必需 DLL 都存在 |
| 5 | 读取 PromeRotation 版本 | 从 `lib/PromeRotation.dll` 提取版本号 |
| 6 | 同步 RotationMetadata 版本 | 将源文件中的版本号更新为本次发布版本 |
| 7 | 编译 | `dotnet build --configuration Release` |
| 8 | 打包 zip | 将 `YourAcre.dll` + `YourAcre.deps.json` 压缩为 `latest.zip` |
| 9 | 生成 repo.json | 创建 PromeRotation 远程 ACR 清单 |
| 10 | 上传 Artifact | 保存产物 |
| 11 | 创建 GitHub Release | 发布 Release |
| 12 | 更新 release 分支 | 将文件推送到 `release` 分支（可选） |

### 关键步骤详解

#### 解析版本号

```yaml
- name: Resolve version
  id: version
  shell: pwsh
  run: |
    $version = "${{ github.event.inputs.version }}"
    if ([string]::IsNullOrWhiteSpace($version)) {
      $version = "${{ github.ref_name }}"
      if ($version.StartsWith("v")) {
        $version = $version.Substring(1)
      }
    }
    $parsedVersion = $null
    if (-not [System.Version]::TryParse($version, [ref]$parsedVersion)) {
      throw "Version '$version' is not a valid System.Version value."
    }
    "version=$version" >> $env:GITHUB_OUTPUT
    "tag=v$version" >> $env:GITHUB_OUTPUT
```

从 tag（`v1.5.2.2` → `1.5.2.2`）或手动输入中提取版本号，并验证格式。

#### 编译

```yaml
- name: Build
  shell: pwsh
  run: |
    dotnet build YourProject.csproj `
      --configuration Release `
      -p:OutputPath="${{ runner.temp }}\publish\" `
      -p:AppendTargetFrameworkToOutputPath=false `
      -p:DalamudReferenceRoot="${{ github.workspace }}\lib" `
      -p:PromeRotationReferenceRoot="${{ github.workspace }}\lib"
```

关键点：通过 `-p` 将引用路径指向仓库中的 `lib/` 目录。

#### 打包

```yaml
- name: Package zip
  shell: pwsh
  run: |
    Compress-Archive `
      -Path "${{ runner.temp }}\publish\YourAcre.dll", "${{ runner.temp }}\publish\YourAcre.deps.json" `
      -DestinationPath "${{ runner.temp }}\dist\latest.zip" `
      -Force
```

只打包 `dll` 和 `deps.json`，引用 DLL 不在其中。

#### 生成 repo.json

```yaml
- name: Generate repo json
  shell: pwsh
  run: |
    $sha256 = (Get-FileHash -Algorithm SHA256 -Path "${{ runner.temp }}\dist\latest.zip").Hash.ToLowerInvariant()

    $manifest = [ordered]@{
      author = "你的名字"
      version = "${{ steps.version.outputs.version }}"
      description = "你的 ACR 描述"
      supportedJobs = @(
        [ordered]@{
          job = "BRD"           # 改成你的职业缩写
          contentScope = "Unspecified"
        }
      )
      apiVersion = [int]"15"                     # PromeRotation API 版本
      referencePromeVersion = "上一步读取的版本"
      downloadUrl = "https://raw.githubusercontent.com/${{ github.repository }}/release/latest.zip"
      sha256 = $sha256
    }

    $json = $manifest | ConvertTo-Json -Depth 8
    Set-Content -Path "${{ runner.temp }}\dist\repo.json" -Value $json -Encoding UTF8
```

这是** PromeRotation 远程 ACR 的专属清单格式**，不是 Dalamud 插件仓库格式。PromeRotation 通过这个 `repo.json` 来发现和下载远程 ACR。

### 完整的 workflow 模板

完整的模板见本教程末尾的附录 A：release.yml 完整模板。

---

## 五、支持版本号同步

你的 Rotation 类上有一个 `[RotationMetadata]` 属性，里面也包含版本号。发布时需要把它和发布版本保持一致。

也就是说 **你无须每次手动更新代码里的版本号， 本工作流会自动根据推送 tag（如 `git tag v1.5.2.2 && git push origin v1.5.2.2`）中的版本号，为你做修改**

工作流中用一个正则替换步骤来完成：

```yaml
- name: Sync RotationMetadata version
  shell: pwsh
  run: |
    $path = "YourJobDir/YourRotation.cs"   # 改成你的源文件路径
    $version = "${{ steps.version.outputs.version }}"
    $text = Get-Content -Path $path -Raw
    $pattern = '(\[RotationMetadata\(\(uint\)Job\.你的职业,\s*"[^"]+",\s*"[^"]+",\s*")[^"]+("\)\])'
    $regex = [regex]::new($pattern)
    $updated = $regex.Replace($text, '$1' + $version + '$2', 1)

    if ($updated -eq $text) {
      throw "Could not find RotationMetadata version in $path."
    }

    Set-Content -Path $path -Value $updated -Encoding UTF8
```

---

## 六、自动生成的 repo.json 格式

`repo.json` 是 PromeRotation 识别远程 ACR 的入口，格式如下：

```json
{
  "author": "你的名字",
  "version": "1.5.2.2",
  "description": "你的 ACR 描述",
  "supportedJobs": [
    {
      "job": "BRD",
      "contentScope": "Unspecified"
    }
  ],
  "apiVersion": 15,
  "referencePromeVersion": "1.0.0.0",
  "downloadUrl": "https://raw.githubusercontent.com/你的用户名/你的仓库/release/latest.zip",
  "sha256": "abcdef..."
}
```

字段说明：

| 字段 | 说明 |
|---|---|
| `author` | 作者名 |
| `version` | 当前 ACR 版本号 |
| `description` | 简短描述 |
| `supportedJobs` | 支持哪些职业（可多职业） |
| `job` | 职业缩写（BRD / MCH / DNC 等） |
| `contentScope` | 内容范围，通常为 `Unspecified` |
| `apiVersion` | PromeRotation API 版本 |
| `referencePromeVersion` | 此 ACR 所基于的 PromeRotation 版本 |
| `downloadUrl` | `latest.zip` 的下载地址 |
| `sha256` | `latest.zip` 的文件哈希 |

---

## 七、触发发布

### 推送 Git Tag

```powershell
git tag v1.5.2.2
git push origin v1.5.2.2
```

tag 名必须为 `v` + 有效的四段版本号（如 `v1.5.2.2`）。


### 发布后的下载链接

每次成功发布后，可从以下链接获取文件：

- `latest.zip`：`https://github.com/<你的用户名>/<你的仓库>/releases/latest/download/latest.zip`
- `repo.json`：`https://github.com/<你的用户名>/<你的仓库>/releases/latest/download/repo.json`

将 `repo.json` 的链接填入 PromeRotation 的远程 ACR 配置界面即可自动加载。

---

## 八、更新 PromeRotation 依赖

当你本地更新了 PromeRotation 后，需要同步更新 `lib/` 中的引用 DLL：

```powershell
# 1. 确保 PromeRotation 已更新到最新版本
# 2. 本地编译（自动把新版 DLL 复制到 lib/）
dotnet build YourProject.csproj

# 3. 提交变更
git add lib/
git commit -m "Update PromeRotation reference to x.x.x.x"
git push
```

CI 会自动从 `lib/PromeRotation.dll` 提取版本号，填入 `repo.json` 的 `referencePromeVersion` 字段。 也无需手动维护。

---


---

## 附录 A：release.yml 完整模板
你可以直接复制使用 

```yaml
name: Build and Release

on:
  workflow_dispatch:
    inputs:
      version:
        description: "Release version, for example 1.5.2.2"
        required: true
        default: "1.0.0.0"
  push:
    tags:
      - "v*"

permissions:
  contents: write

env:
  ACR_AUTHOR: 你的名字
  ACR_DESCRIPTION: 你的 ACR 描述
  ACR_JOB: BRD              # 改成你的职业缩写
  ACR_CONTENT_SCOPE: Unspecified
  ACR_API_VERSION: "15"

jobs:
  release:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Resolve version
        id: version
        shell: pwsh
        run: |
          $version = "${{ github.ref_name }}"
          if ($version.StartsWith("v")) {
            $version = $version.Substring(1)
          }
          $parsedVersion = $null
          if (-not [System.Version]::TryParse($version, [ref]$parsedVersion)) {
            throw "Version '$version' is not a valid System.Version value."
          }
          "version=$version" >> $env:GITHUB_OUTPUT
          "tag=v$version" >> $env:GITHUB_OUTPUT

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"    # 改成你的 .NET 版本

      - name: Check reference DLLs
        id: references
        shell: pwsh
        run: |
          $required = @(
            "Dalamud.dll",
            "Dalamud.Bindings.ImGui.dll",
            "FFXIVClientStructs.dll",
            "Lumina.dll",
            "Lumina.Excel.dll",
            "ECommons.dll",
            "PromeRotation.dll"
          )
          $missing = $required | Where-Object { -not (Test-Path "lib/$_") }
          if ($missing.Count -gt 0) {
            throw "Missing reference DLLs in lib/: $($missing -join ', ')"
          }
          $promeDll = Resolve-Path "lib/PromeRotation.dll"
          $promeVersion = [System.Reflection.AssemblyName]::GetAssemblyName($promeDll).Version.ToString()
          "prome_version=$promeVersion" >> $env:GITHUB_OUTPUT

      - name: Sync RotationMetadata version
        shell: pwsh
        run: |
          $path = "你的职业目录/YourRotation.cs"    # 改成你的源文件路径
          $version = "${{ steps.version.outputs.version }}"
          $text = Get-Content -Path $path -Raw
          $pattern = '(\[RotationMetadata\(\(uint\)Job\.BRD,\s*"[^"]+",\s*"[^"]+",\s*")[^"]+("\)\])'
          $regex = [regex]::new($pattern)
          $updated = $regex.Replace($text, '$1' + $version + '$2', 1)
          if ($updated -eq $text) {
            throw "Could not find RotationMetadata version in $path."
          }
          Set-Content -Path $path -Value $updated -Encoding UTF8

      - name: Build
        shell: pwsh
        run: |
          dotnet build YourProject.csproj `
            --configuration Release `
            -p:OutputPath="${{ runner.temp }}\publish\" `
            -p:AppendTargetFrameworkToOutputPath=false `
            -p:DalamudReferenceRoot="${{ github.workspace }}\lib" `
            -p:PromeRotationReferenceRoot="${{ github.workspace }}\lib"

      - name: Package zip
        shell: pwsh
        run: |
          $dist = "${{ runner.temp }}\dist"
          New-Item -ItemType Directory -Force -Path $dist | Out-Null
          Compress-Archive `
            -Path "${{ runner.temp }}\publish\YourAcre.dll", "${{ runner.temp }}\publish\YourAcre.deps.json" `
            -DestinationPath "$dist\latest.zip" `
            -Force

      - name: Generate repo json
        shell: pwsh
        run: |
          $dist = "${{ runner.temp }}\dist"
          $sha256 = (Get-FileHash -Algorithm SHA256 -Path "$dist\latest.zip").Hash.ToLowerInvariant()
          $manifest = [ordered]@{
            author = "${{ env.ACR_AUTHOR }}"
            version = "${{ steps.version.outputs.version }}"
            description = "${{ env.ACR_DESCRIPTION }}"
            supportedJobs = @(
              [ordered]@{
                job = "${{ env.ACR_JOB }}"
                contentScope = "${{ env.ACR_CONTENT_SCOPE }}"
              }
            )
            apiVersion = [int]"${{ env.ACR_API_VERSION }}"
            referencePromeVersion = "${{ steps.references.outputs.prome_version }}"
            downloadUrl = "https://raw.githubusercontent.com/${{ github.repository }}/release/latest.zip"
            sha256 = $sha256
          }
          $json = $manifest | ConvertTo-Json -Depth 8
          Set-Content -Path "$dist\repo.json" -Value $json -Encoding UTF8

      - name: Upload workflow artifact
        uses: actions/upload-artifact@v4
        with:
          name: release-files
          path: |
            ${{ runner.temp }}\dist\latest.zip
            ${{ runner.temp }}\dist\repo.json

      - name: Publish GitHub release
        uses: softprops/action-gh-release@v2
        continue-on-error: true
        with:
          tag_name: ${{ steps.version.outputs.tag }}
          target_commitish: ${{ github.sha }}
          name: ${{ steps.version.outputs.tag }}
          make_latest: true
          files: |
            ${{ runner.temp }}\dist\latest.zip
            ${{ runner.temp }}\dist\repo.json

      - name: Publish stable download branch
        uses: peaceiris/actions-gh-pages@v4
        with:
          github_token: ${{ github.token }}
          publish_branch: release
          publish_dir: ${{ runner.temp }}\dist
          force_orphan: true
```



