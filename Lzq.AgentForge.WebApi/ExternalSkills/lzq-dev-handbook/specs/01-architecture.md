# 项目架构概览

## 解决方案结构

```
WorkBuddy/
├── code/
│   ├── Directory.Build.props          # 统一版本管理
│   ├── NuGet.Config                    # 包源配置
│   ├── Host/                           # WebApi 启动项目
│   └── Modules/                        # 业务模块
│       ├── BaseData/                   # 基础数据模块
│       ├── Dashboard/                  # 看板模块
│       ├── Equipment/                  # 设备管理模块
│       ├── QA/                         # 质量管理模块
│       └── WorkOrder/                  # 工单模块
├── problems/                           # 问题记录
├── docs/                               # 文档
└── scripts/                            # 脚本
```

## 模块三层架构

```
Lzq.{Module}.Domain/                  # 领域层（含 Repository 实现）
├── Entities/{Category}/              # 实体（每个实体一个独立文件）
├── Enums/                            # 枚举
├── IRepositories/{Category}/         # 仓储接口
├── Repositories/{Category}/          # 仓储实现 ✅（当前项目架构决策）
└── SeedDatas/                        # 种子数据

Lzq.{Module}.Application.Contracts/   # 契约层
├── {Category}/
│   ├── Commands/                     # 命令（Create/Update/Delete）
│   ├── Queries/                      # 查询（Page/List/Get）
│   ├── Dto/                         # 数据传输对象
│   └── Validators/                   # 验证器
└── IServices/                        # 服务接口

Lzq.{Module}.Application/             # 应用层
└── Services/{Category}/              # 服务实现
```

## 架构决策：Repository 在 Domain 层 ✅

**当前决策**：Repository 实现放置在 Domain 层的 `Repositories/` 目录下。

**理由**：
- 项目只使用 SqlSugar 作为 ORM，不计划扩展多数据库
- 避免创建独立的 Infrastructure 层，简化架构
- Repository 实现与 SqlSugar 强耦合，放 Domain 层更直观

**依赖方向**：
```
WebApi (Host)
  ↓ 引用
Application (服务实现)
  ↓ 引用
Application.Contracts (接口/DTO)
  ↓ 引用
Domain (实体/仓储/种子数据)
```

**禁止行为**：
- ❌ Contracts 层引用 Application 层
- ❌ Domain 层引用其他模块的 Domain 层
- ❌ 跨模块直接使用其他模块的 Domain 层仓储

---

## 环境搭建与配置

### 版本管理 (Directory.Build.props)

```xml
<Project>
  <PropertyGroup>
    <LzqExtensionsVersion>0.1.41</LzqExtensionsVersion>
    <MASAFrameworkVersion>1.2.0-preview.10</MASAFrameworkVersion>
    <MicrosoftPackageVersion>10.*</MicrosoftPackageVersion>
  </PropertyGroup>
</Project>
```

### NuGet 包源配置 (NuGet.Config)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="local" value="D:\gitee\Lzq.Extensions\packages" />
  </packageSources>
</configuration>
```

### 模块 .csproj 必备属性

每个模块项目文件必须包含完整的 `PropertyGroup`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <RootNamespace>Lzq.{Module}.{Layer}</RootNamespace>
        <AssemblyName>Lzq.{Module}.{Layer}</AssemblyName>
    </PropertyGroup>
</Project>
```

### 包引用指南

| 层 | 引用的包 |
|---|---|
| **Domain** | `Lzq.Extensions.SqlSugar` |
| **Application.Contracts** | `Lzq.Extensions.Jwt`、`Lzq.Extensions.ExternalHttpApi` |
| **Application** | `Lzq.Core`、`Lzq.Extensions.NSwag` |

---

## 命名规范

| 元素 | 规范 | 示例 |
|---|---|---|
| 项目名称 | `Lzq.{Module}.{Layer}` | `Lzq.Rbac.Domain` |
| 实体类 | `{Entity}Entity` | `UserEntity` |
| DTO | `{Entity}{Action}Dto` / `{Entity}ViewDto` | `UserViewDto` |
| Command/Query | 动词+名词+类型 | `CreateUserCommand` |
| 服务接口 | `I{Module}Service` | `IUserService` |
| 服务实现 | `{Module}Service` | `UserService` |
| 仓储接口 | `I{Entity}Repository` | `IUserRepository` |
| 异步方法 | 以 `Async` 结尾 | `GetUserAsync()` |

## 实体文件组织

- ✅ 每个实体类独立一个 .cs 文件
- ✅ 文件名与类名一致
- ❌ 多个实体放在同一个文件中

```
Entities/{Category}/
├── PlanEntity.cs    ✅
├── RecordEntity.cs  ✅
└── ItemEntity.cs    ✅
```
