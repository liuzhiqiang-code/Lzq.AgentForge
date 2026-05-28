# AI 模块优化分析

> 生成日期: 2026-05-26
> 扫描范围: AI Application / Application.Contracts / Domain + 前端 vue + Lzq.Extensions.AI / Redis

---

## 🔴 高优先级

### H1 — 分析看板全部为模拟数据

- **位置**: `AnalyticsService.cs:28-123`
- **问题**: `TodayApiCalls`、`TotalApiCalls`、`TodaySkillCalls`、对话趋势、月度模型调用量、模型分布、技能统计等 7 个接口全部使用 `Random.Shared.Next()` 或硬编码数据
- **影响**: 整个 AI 运营分析功能不可用
- **建议**: 基于 `ModelRunRecordEntity` / `ChatsEntity` 实现真实聚合统计（SqlSugar GroupBy + 日期函数按日月统计）

### H2 — AES 密钥来源不一致

- **位置**: `ApiKeyCrypto.cs:8`
- **问题**: `Environment.GetEnvironmentVariable("AES_KEY") ?? "AES_KEY"` — 环境变量 fallback 值为 `"AES_KEY"` 是严重安全隐患；同时 `ApiKeyService.cs:29-30` 使用 `Configuration.GetSection("AesKey")`，两套读取方式不一致
- **影响**: API Key 加密安全性
- **建议**: 统一使用 `IConfiguration` 注入获取 AesKey，移除环境变量分支

### H3 — 技能管理 CRUD 空实现

- **位置**: `AgentSkillService.cs:28-58`
- **问题**: `CreateAsync`、`UpdateAsync`、`DeleteAsync`、`BatchDeleteAsync` 四个方法体内代码全部注释，仅返回 `ApiResult.Success()`
- **影响**: 前端技能管理增删改按钮无实际作用
- **建议**: 补齐数据库持久化逻辑，或标记 `[Obsolete]` 返回 501

### H4 — SSE 流式无超时保护

- **位置**: `ChatsService.cs:349-402`
- **问题**: `CompletionAsync` 没有请求超时。AI 模型卡死时连接一直保持，导致资源泄漏
- **影响**: 服务端线程/连接池耗尽风险
- **建议**: `CancellationTokenSource.CreateLinkedTokenSource(ct)` 附加 5 分钟超时

### H5 — 前端 `getConfiguredModels` 路由后端不存在

- **位置**: `api/apiKey.ts:53-55`
- **问题**: 前端调用 `GET /ai/apiKey/getConfiguredModels`，后端无此路由
- **影响**: 功能不可用
- **建议**: 删除该函数引用或实现后端对应路由

### H6 — 前端 `finally` 未释放 `isStreaming` 锁

- **位置**: `index.vue:412-421`
- **问题**: `catch (AbortError)` 中 `return` 后未执行解锁，`finally` 中也没有 `isStreaming = false`
- **影响**: 连接异常中断时用户永远无法发送新消息
- **建议**: finally 块必须设置 `isStreaming = false`；收到 `close` 事件后调用 `reader.cancel()` + break

---

## 🟡 中优先级

### M1 — 模型厂商扩展性差

- **位置**: `ApiKeyService.cs:93-138` + `ProviderEnum.cs`
- **问题**: 只支持 DeepSeek/SiliconFlow 两家；switch 硬编码，添加新厂商需改多处
- **建议**: 策略模式 `IProviderStrategy`，或字典注册 Provider URL

### M2 — ChatClient 缓存泄漏

- **位置**: `ChatClientFactory.cs:12-43`
- **问题**: `ConcurrentDictionary<string, IChatClient>` 永远不清理，API Key 更新/模型配置变更后旧客户端仍占用内存
- **影响**: 长期运行内存增长；配置更新不生效
- **建议**: 加版本号或定期清理，配置变更时主动 `ClearAndDisposeClients()`

### M3 — 模型配置测试连接时 ConfigId 错误

- **位置**: `AIModelConfigService.cs:87-107`
- **问题**: `CreateAsync` 先测试连接再插入，测试时 `entity.Id = 0`，ConfigId 拼接为 `KeyName_DisplayModelName_0`，后续 ID 变更后不匹配
- **建议**: 先 Insert 获取 ID 后再测试连接

### M4 — API Key Update 不能修改密钥值

- **位置**: `ApiKeyService.cs:183-201`
- **问题**: `UpdateAsync` 只更新 `KeyName` 和 `IsEnabled`，无法修改 `KeyValue`、`Provider`、`BaseUrl`
- **建议**: UpdateCommand 支持重新输入 KeyValue，重新加密存储

### M5 — 前端 SSE close 事件未释放资源

- **位置**: `index.vue:405-408`
- **问题**: `close` 事件仅设置 `isStreaming = false`，未调用 `reader.cancel()`
- **建议**: 收到 `close` 后 `reader.cancel()` + break

### M6 — Token 用量统计缺失

- **位置**: `ChatsService.cs` + `ChatContentDto.cs` + `ModelRunRecordEntity.cs`
- **问题**: `PromptTokens`/`CompletionTokens` 字段存在但从未赋值
- **建议**: 从 `ChatUsage` 提取并回填

### M7 — 智能体技能选择器 API 为空

- **位置**: `agentManage/data.ts:105-107`
- **问题**: `ApiSelect` 的 `api` 返回空数组 `TODO: 获取技能列表的 API`
- **建议**: 接入 `/ai/agentSkill/list` 接口

### M8 — 语音识别失败无用户反馈

- **位置**: `voice-input-button.vue:51`
- **问题**: `message.error('语音识别失败')` 被注释掉
- **建议**: 取消注释，恢复错误提示

---

## 🟢 低优先级

### L1 — 无效的死 CSS 样式

- **位置**: `index.vue` 第二个 `<style scoped>`（约行 581-643）
- **问题**: `.thinking-block`、`.tool-block`、`.markdown-body` 等样式由于 scoped 作用域不会穿透到子组件 `chatMessage.vue`，全部无效
- **建议**: 删除该 `<style scoped>` 块

### L2 — 旧版页面残留

- **位置**: `chats/index2.vue`（1293 行）
- **问题**: 与 `index.vue`（643 行）并行存在的旧版备份
- **建议**: 清理

### L3 — 语音转文字接口无认证

- **位置**: `ChatsService.cs:595` — `[AllowAnonymous]`
- **问题**: 接口可被任意调用
- **建议**: 移除 `[AllowAnonymous]` 或加频率限制

### L4 — `computed` 导入未合并

- **位置**: `index.vue:2` + `index.vue:56`
- **问题**: `import { computed } from 'vue'` 单独一行，可合并到顶部 import

### L5 — 生产环境 debug 日志

- **位置**: `voice-input-button.vue:47` — `console.log('语音识别结果:', result)`
- **建议**: 移除

### L6 — CSS content 动画不工作

- **位置**: `chatMessage.vue:269-273`
- **问题**: `content` 属性不支持 CSS 动画，加载中 `...` 始终静态
- **建议**: 改用 `visibility` 或 `opacity` 动画
