---
name: lzq-dev-handbook
description: Lzq 后端开发手册 - 涵盖 .NET 10 项目架构、SqlSugar ORM、模块化开发模式及 Lzq.Extensions 全家桶集成方法。基于 WorkBuddy (Lzq.MES) 实际项目经验与 168 个已验证问题汇总。
license: Internal use
---


# Lzq 后端开发手册

Lzq 框架下 .NET 后端开发的完整参考指南，覆盖项目架构、核心库使用、模块开发流程、12 个扩展库集成及已验证的已知问题解决方案。

## 目的

为 Lzq 框架后端开发提供标准化规范，确保代码风格、命名空间、测试模式一致，避免重复踩坑。

## 使用流程

当用户提出后端开发相关问题时，按以下流程处理：

1. **识别问题类型**：判断属于架构规范、核心库、模块开发、质量保障、扩展库集成、还是已知问题
2. **定位文档**：根据下方"快速查找"表或扩展库触发规则找到对应的子文档
3. **读取文档**：使用 Read 工具打开对应文件，基于文档内容回答，不要凭记忆猜测
4. **交叉参考**：如果问题涉及编译错误/命名空间，同时检查 `problems/KNOWN-ISSUES.md`

## 规范文档

4 份核心规范，位于 `specs/` 目录。触发规则如下：

### 1. 项目架构
**何时读取**：用户问及解决方案结构、模块分层、目录组织、环境配置、命名规范
**文档**：[`specs/01-architecture.md`](./specs/01-architecture.md)
**典型提问**："项目是怎么分层的？""模块有哪些项目？""引用关系是怎样的？"

### 2. 核心库
**何时读取**：用户问及 ServiceBase 基类、ApiResult 返回值、参数绑定、C# 可空类型
**文档**：[`specs/02-core-library.md`](./specs/02-core-library.md)
**典型提问**："ServiceBase 怎么用？""ApiResult<T> 怎么返回？""可空类型怎么写？"

### 3. 模块开发
**何时读取**：用户要求创建新模块、添加实体/仓储/服务、配置依赖注入
**文档**：[`specs/03-module-dev.md`](./specs/03-module-dev.md)
**典型提问**："怎么创建一个新模块？""怎么定义实体？""仓储怎么实现？"

### 4. 质量保障
**何时读取**：用户问及单元测试、代码审查、ServiceTestBase、命名空间速查
**文档**：[`specs/04-quality.md`](./specs/04-quality.md)
**典型提问**："怎么写单元测试？""测试基类怎么配置？""命名空间应该用哪个？"

## 扩展库集成

12 个扩展库的详细文档，位于 `extensions/` 目录。**当用户问题涉及以下关键词时，必须读取对应文档**：

| 用户提问关键词 | 读取文档 |
|---|---|
| ORM、实体、Entity、仓储、Repository、BaseFullEntity、SqlSugar、SugarColumn、CodeFirst | [`extensions/sqlsugar.md`](./extensions/sqlsugar.md) |
| 事件、EventBus、CQRS、命令、Command、查询、Query | [`extensions/eventbus.md`](./extensions/eventbus.md) |
| 日志、Log、Serilog、结构化日志 | [`extensions/serilog.md`](./extensions/serilog.md) |
| 认证、授权、JWT、Token、登录 | [`extensions/jwt.md`](./extensions/jwt.md) |
| API文档、Swagger、NSwag、OpenAPI | [`extensions/nswag.md`](./extensions/nswag.md) |
| 缓存、Redis、分布式锁、Cache | [`extensions/redis.md`](./extensions/redis.md) |
| HTTP、外部调用、ExternalHttpApi、HttpClient | [`extensions/external-http.md`](./extensions/external-http.md) |
| 消息队列、RabbitMQ、MQ、异步消息 | [`extensions/rabbitmq.md`](./extensions/rabbitmq.md) |
| 设计模式、核心模式、约定 | [`extensions/core-patterns.md`](./extensions/core-patterns.md) |
| 模块开发详解、Module | [`extensions/module-development.md`](./extensions/module-development.md) |
| AgentForge、参考架构 | [`extensions/agentforge-webapi.md`](./extensions/agentforge-webapi.md) |
| 质量保障、Quality、规范 | [`extensions/development-quality.md`](./extensions/development-quality.md) |

**重要**：如果用户问题匹配多个关键词，优先读取最相关的文档。遇到编译/运行时错误时，在读取扩展库文档之前，先检查 `problems/KNOWN-ISSUES.md`。

## 已知问题

168 个已验证的编译/运行时问题，按轮次组织，包含根因分析、修复方案和预防措施。

**何时读取**：用户遇到编译错误、运行时异常、命名空间报错、测试失败时
**文档**：[`problems/KNOWN-ISSUES.md`](./problems/KNOWN-ISSUES.md)
**典型提问**："编译报错说找不到 BaseFullEntity""ISqlSugarRepository 未注册""CreatedTime 编译失败"

## 文件编码

所有 .cs 文件必须使用 UTF-8 无 BOM 编码。

**何时读取**：写入 .cs 文件后发现中文乱码，或需要确认编码规范时
**文档**：[`problems/encoding.md`](./problems/encoding.md)
**典型提问**：".cs 文件中文乱码了""文件编码应该用什么格式"

## 架构评估

框架架构评估与改进建议（抽象层设计、可观测性、运维能力等）。

**何时读取**：用户问及框架设计、架构评估、改进方向时
**文档**：[`problems01/`](./problems01/)
**典型提问**："框架有什么可以改进的？""抽象层怎么设计？"

---

## 快速查找

| 用户问题关键词 | 对应文档 |
|---|---|
| 架构、分层、项目结构、解决方案 | [`specs/01-architecture.md`](./specs/01-architecture.md) |
| ServiceBase、ApiResult、参数绑定 | [`specs/02-core-library.md`](./specs/02-core-library.md) |
| 创建模块、实体、仓储、服务 | [`specs/03-module-dev.md`](./specs/03-module-dev.md) |
| 测试、xUnit、ServiceTestBase | [`specs/04-quality.md`](./specs/04-quality.md) |
| ORM、Entity、Repository、SqlSugar | [`extensions/sqlsugar.md`](./extensions/sqlsugar.md) |
| 日志、Serilog | [`extensions/serilog.md`](./extensions/serilog.md) |
| 缓存、Redis | [`extensions/redis.md`](./extensions/redis.md) |
| 认证、JWT、Token | [`extensions/jwt.md`](./extensions/jwt.md) |
| 编译报错、运行时错误、异常 | [`problems/KNOWN-ISSUES.md`](./problems/KNOWN-ISSUES.md) |
| 编码、乱码、UTF-8 | [`problems/encoding.md`](./problems/encoding.md) |
| 架构评估、改进 | [`problems01/`](./problems01/) |

> 如果问题不在上述列表中，优先检查 `problems/KNOWN-ISSUES.md`，再按需搜索 `specs/` 和 `extensions/` 目录。
