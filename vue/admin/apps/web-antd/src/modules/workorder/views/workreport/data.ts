import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { WorkReportApi } from '#/modules/workorder/api/workreport';
import { getWorkOrderPage } from '#/modules/workorder/api/workorder';

import { $t } from '#/locales';

/**
 * 获取编辑表单字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        api: async () => {
          const res = await getWorkOrderPage({ pageNum: 1, pageSize: 1000 });
          return (res.items || []).map((item: any) => ({
            label: `${item.code} - ${item.productName || ''}`,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('workorder.workreport.workOrderPlaceholder'),
      },
      fieldName: 'workOrderId',
      label: $t('workorder.workreport.workOrderId'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        // TODO: 替换为实际的用户API
        api: async () => {
          // 示例: const res = await getUserPage({ pageNum:1, pageSize:1000 });
          return [{
            label: 1,
            value: "111"
          }];
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('workorder.workreport.operatorPlaceholder'),
      },
      fieldName: 'operatorId',
      label: $t('workorder.workreport.operatorId'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'qualifiedQty',
      label: $t('workorder.workreport.qualifiedQty'),
      rules: 'required',
      defaultValue: 0,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'defectQty',
      label: $t('workorder.workreport.defectQty'),
      defaultValue: 0,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
        min: 0, step: 0.5 
      },
      fieldName: 'workHours',
      label: $t('workorder.workreport.workHours'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'reportTime',
      label: $t('workorder.workreport.reportTime'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'remark',
      label: $t('workorder.workreport.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<WorkReportApi.WorkReportItem>,
): VxeTableGridOptions<WorkReportApi.WorkReportItem>['columns'] {
  return [
    { field: 'workOrderCode', title: $t('workorder.workreport.workOrderId'), width: 150 },
    { field: 'operatorName', title: $t('workorder.workreport.operatorId'), width: 120 },
    { field: 'qualifiedQty', title: $t('workorder.workreport.qualifiedQty'), width: 100 },
    { field: 'defectQty', title: $t('workorder.workreport.defectQty'), width: 100 },
    { field: 'workHours', title: $t('workorder.workreport.workHours'), width: 100 },
    { field: 'reportTime', title: $t('workorder.workreport.reportTime'), width: 160 },
    { field: 'remark', title: $t('workorder.workreport.remark'), minWidth: 180 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'workOrderCode', onClick: onActionClick },
        name: 'CellOperation',
        options: ['delete'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('workorder.workreport.operation'),
      width: 150,
    },
  ];
}
