# Upload - 文件上传

## 用法示例

```typescript
{
  component: 'Upload',
  componentProps: {
    accept: '.png,.jpg,.jpeg',
    customRequest: upload_file,      // 调用项目的上传 API
    maxCount: 1,
    maxSize: 2,                      // MB
    multiple: false,
    showUploadList: true,
    listType: 'picture-card',
    crop: true,                      // 裁剪图片（可选）
    aspectRatio: '1:1',
    handleChange: ({ file }) => {
      const { name, status } = file;
      if (status === 'done') {
        message.success(`${name} 上传成功`);
      } else if (status === 'error') {
        message.error(`${name} 上传失败`);
      }
    },
  },
  fieldName: 'files',
  label: '上传图片',
}
```

## 关键点
- `customRequest` 传入项目的上传 API 函数
- `maxSize` 单位为 MB
- `listType: 'picture-card'` 显示图片卡片预览
- `crop: true` 启用图片裁剪（需配合 `aspectRatio`）
