# 12 - 开发质量保障参考

> 来源: lzq-development-quality | 状态: ✅ 已验证

## AI 审查流程

### 步骤 1：静态规范审查

| 检查项 | 说明 |
|---|---|
| 实体继承 | BaseFullEntity（非 BaseEntity）|
| 实体文件 | 每个实体独立一个 .cs 文件 |
| 命名空间 | 所有 using 正确（对照速查表）|
| 接口实现 | Service 实现对应接口 |
| 返回类型 | ApiResult<T> 匹配接口定义 |
| 接口特性 | 无 [FromBody]（除非远程调用） |
| 排序 API | 使用链式 OrderByDescending |
| 验证器 | using FluentValidation |
| csproj | 完整 PropertyGroup |

### 步骤 2：编译验证

- 检查编译错误
- 关联错误的 Skill 文档
- 生成诊断报告到 `problems/`

### 步骤 3：错误诊断

| 错误类型 | 检查方向 |
|---|---|
| 缺少包引用 | .csproj 及 Directory.Build.props |
| 类型不存在 | using 语句或项目引用 |
| 方法签名不匹配 | 接口与实现一致性 |
| 命名空间错误 | 对照速查表 |

## 问题报告格式

```markdown
# 代码审查报告 - {模块名称}

**审查时间**: {timestamp}
**模块路径**: {相对路径}

## 概览
- 总文件数: {n}
- 问题总数: {m}
- 严重级别: 🔴 阻塞 | 🟠 重要 | 🟡 建议

## 问题列表
(具体问题)
```

## 架构问题处理

🚨 发现架构问题：记录 → 说明原因 → **等待决策** → 执行
❌ 禁止自行修改架构决策
