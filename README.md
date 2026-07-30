# Wotou-TC

Wotou-TC 是基于 PromeRotation 框架的 FFXIV **吟游诗人（BRD）** 自动循环（ACR），面向 **繁中服（台服）Dalamud API13** 环境。

## 目标环境

- .NET 9 / `net9.0-windows`
- Dalamud API 13（yanmucorp 维护的繁中服版本）
- PromeRotation API13

## 快速开始

```powershell
git clone <本仓库>
dotnet build WotouTC.csproj
```

项目通过 NuGet 包 `PromeRotation.SDK.TC` 获取编译期引用，无需手动配置 DLL 路径。

## 项目结构

| 目录/文件 | 说明 |
|---|---|
| `Bard/` | 吟游诗人 ACR 逻辑（技能、Buff、开场、战斗数据） |
| `docs/` | 开发文档 |
| `.github/workflows/release.yml` | GitHub Actions 发布工作流 |
| `WotouTC.csproj` | 项目文件，引用 PromeRotation.SDK.TC |

## 与 Wotou（国服版）的关系

Wotou-TC 是 Wotou（国服版）的台服 API13 移植版本，使用独立仓库管理。

## 许可证

见仓库根目录。
