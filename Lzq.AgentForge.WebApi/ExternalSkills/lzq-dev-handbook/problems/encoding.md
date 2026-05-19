# 文件编码规范

---

## 问题背景

本机环境下 PowerShell `Set-Content` 和 WorkBuddy Write Tool 会输出 **GBK 编码**，导致 .cs 文件出现乱码或编译错误。

---

## 规范要求

### 所有 .cs 文件必须使用 UTF-8 无 BOM 编码

| 场景 | 正确做法 | 错误做法 |
|---|---|---|
| 手动创建 .cs 文件 | 使用 UTF-8 无 BOM 编码保存 | 使用 GBK/UTF-8 with BOM |
| PowerShell 写入含中文的 .cs 文件 | 使用 `[System.IO.File]::WriteAllText()` | 使用 `Set-Content` |
| Write Tool 写入含中文的 .cs 文件 | 写入后必须用 `WriteAllText` 重新保存 | 直接保存 |

---

## PowerShell 正确写法

```powershell
# ✅ 正确 - 使用 UTF-8 无 BOM 编码
[System.IO.File]::WriteAllText(
    $filePath, 
    $content, 
    [System.Text.UTF8Encoding]::new($false)  # $false = 无 BOM
)

# ❌ 错误 - PowerShell 默认输出 GBK 编码
Set-Content -Path $filePath -Value $content
```

---

## 验证文件编码

```powershell
# 检查文件编码
[System.IO.StreamReader]::new($filePath).CurrentEncoding
```

---

## 编码对照表

| 编码 | BOM | PowerShell `Set-Content` | `WriteAllText(..., UTF8Encoding)` |
|---|---|---|---|
| UTF-8 无 BOM | 无 | ❌ 输出 GBK | ✅ `$false` |
| UTF-8 with BOM | `EF BB BF` | ❌ 输出 GBK | ⚠️ `$true` |
| GBK (936) | 无 | ✅ 默认 | ❌ 需指定 `Encoding.GetEncoding(936)` |

---

## C# 文件中的中文处理

### 字符串常量
```csharp
// ✅ 正确 - 字符串常量直接使用
public class ProcessEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
    public string? Description { get; set; }  // 可选字段，可包含中文
}
```

### 验证器错误消息
```csharp
// ✅ 正确 - 错误消息使用中文
public class ProcessCreateCommandValidator : AbstractValidator<ProcessCreateCommand>
{
    public ProcessCreateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("名称不能为空");
    }
}
```

---

## 注意事项

1. **GBK 编码问题**：PowerShell 和某些工具的默认编码是 GBK（代码页 936），这会导致中文乱码。
2. **BOM 问题**：UTF-8 with BOM 在某些编译器或工具中可能导致问题，建议始终使用无 BOM 格式。
3. **跨平台兼容性**：UTF-8 无 BOM 是跨平台的标准格式，推荐使用。

---

## 相关工具

如果需要批量转换文件编码，可以使用以下工具：

```powershell
# 使用 PowerShell 转换单个文件
$content = Get-Content $filePath -Raw
[System.IO.File]::WriteAllText($filePath, $content, [System.Text.UTF8Encoding]::new($false))

# 批量转换目录下所有 .cs 文件（谨慎使用）
Get-ChildItem -Path "D:\gitee\WorkBuddy\code" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.UTF8Encoding]::new($false))
}
```

> ⚠️ **警告**：批量转换前请先备份文件，避免意外损坏。

---

> 📝 **参考**：此规范源于实际项目中遇到的 PowerShell 编码问题，已固化到开发流程中。
