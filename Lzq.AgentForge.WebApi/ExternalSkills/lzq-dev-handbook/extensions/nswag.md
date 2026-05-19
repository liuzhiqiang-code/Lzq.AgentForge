# 03 - NSwag API 文档参考

> 来源: lzq-extensions-nswag | 状态: ✅ 已验证

## 外部文档聚合

```csharp
new SwaggerDocumentInfo
{
    Name = "external-service",
    Title = "外部 API",
    ExternalUrl = "https://remote-api/swagger/v1/swagger.json"
}
```

> 外部文档从浏览器端直接获取，需确保远端服务器可达且 CORS 配置正确。

## 密码保护流程

1. 访问 `/swagger` → 显示密码表单
2. POST `/swagger-password-verify` → 设置签名 Cookie (SHA-256)
3. 后续请求携带 Cookie 绕过验证

## JWT 安全配置

`EnableJwtSecurity = true` 时：
- 自动添加 `"JWT"` 安全方案到 OpenAPI 文档
- Swagger UI 显示"授权"按钮
- 用户粘贴 Token（无需 `Bearer` 前缀）

## 常见问题

| 问题 | 解决方案 |
|---|---|
| "No service for type IOptions<NSwagOptions>" | 确保在 Build 前调用 `AddLzqNSwag()` |
| 外部文档未显示 | 检查 CORS、远端可达性 |
| 密码保护未生效 | 检查 Cookie 路径与 BasePath 匹配 |
