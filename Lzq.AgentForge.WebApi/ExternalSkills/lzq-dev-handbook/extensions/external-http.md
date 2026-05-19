# 05 - ExternalHttpApi 参考

> 来源: lzq-extensions-externalhttpapi | 状态: ✅ 已验证

## 配置键名规则

默认：接口名去掉首字母 `I`（如 `IUserApi` → `UserApi`）
自定义：使用 `[ExternalHttpApiConfig("MyKey")]` 特性

## AOP 过滤器

### ApiLoggingAttribute
- 请求时：记录 `请求地址: {Url}`
- 响应时：记录 `响应状态: {StatusCode}`

### ApiReturnUnwrapperAttribute
- 检查 HTTP 状态码 → 反序列化为 `ApiResult<T>` → 若 `Code != 200` 抛异常 → 返回 `data`
- 前提：外部 API 返回 `{ code, message, data }` 结构

### HttpContextHeaderFilter（全局）
触发条件：接口标注 `[NeedAuthToken]`
透传头：
- `X-Trace-Id` ← `HttpContext.TraceIdentifier`
- `Authorization` ← 原始请求头
- `x-tenant-id` ← `__tenant__`

## 启动校验
缺少 `ExternalApis` 配置或 `HttpHost` 无效 → `InvalidOperationException`，拒绝启动

## 程序集扫描
默认使用 `MasaApp.GetAssemblies()`，可手动传入：
```csharp
services.AddExternalHttpApis(configuration, typeof(MyApi).Assembly);
```
