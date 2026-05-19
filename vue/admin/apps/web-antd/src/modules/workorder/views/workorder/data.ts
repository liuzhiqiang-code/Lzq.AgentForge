import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { WorkOrderApi } from '#/modules/workorder/api/workorder';
import { getLinePage } from '#/modules/basadata/api/line';
import { getProcessListByLine } from '#/modules/basadata/api/process';

import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('workorder.workorder.code'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'productName',
      label: $t('workorder.workorder.productName'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        class: 'w-full',
        api: async () => {
          const res = await getLinePage({ pageNum: 1, pageSize: 1000 });
          return (res.items || []).map((item) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
        showSearch: true,
        allowClear: true,
        placeholder: $t('workorder.workorder.linePlaceholder'),
      },
      fieldName: 'lineId',
      label: $t('workorder.workorder.lineId'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
      // 👇 产线变化时，清空已选工序
      dependencies: {
        trigger(values, form) {
          form.setFieldValue('processId', undefined);
        },
        triggerFields: ['lineId'],
      },
    },
    {
      component: 'ApiSelect',
      fieldName: 'processId',
      label: $t('workorder.workorder.processId'),
      formItemClass: 'md:col-span-1 col-span-2',
      // 👇 根据当前 lineId 动态加载工序列表
      dependencies: {
        componentProps(values) {
          return {
            class: 'w-full',
            showSearch: true,
            allowClear: true,
            placeholder: $t('workorder.workorder.processPlaceholder'),
            // 将 lineId 作为参数传给 api
            params: { lineId: values.lineId },
            api: async ({ lineId }) => {
              if (!lineId) return [];
              const res = await getProcessListByLine(lineId);
              return (res || []).map((item) => ({
                label: `${item.code} - ${item.name}`,
                value: item.id,
              }));
            },
          };
        },
        triggerFields: ['lineId'],
        // 可选：未选产线时禁用工序下拉
        disabled(values) {
          return !values.lineId;
        },
      },
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'plannedQty',
      label: $t('workorder.workorder.plannedQty'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps:{
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'plannedStart',
      label: $t('workorder.workorder.plannedStart'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps:{
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'plannedEnd',
      label: $t('workorder.workorder.plannedEnd'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
  ];
}

const statusOptions = [
  { value: 0, label: $t('workorder.workorder.statusDraft'), color: 'default' },
  { value: 1, label: $t('workorder.workorder.statusDispatched'), color: 'processing' },
  { value: 2, label: $t('workorder.workorder.statusInProgress'), color: 'warning' },
  { value: 3, label: $t('workorder.workorder.statusCompleted'), color: 'success' },
  { value: 4, label: $t('workorder.workorder.statusClosed'), color: 'default' },
  { value: 5, label: $t('workorder.workorder.statusCancelled'), color: 'error' },
  { value: 6, label: $t('workorder.workorder.statusPaused'), color: 'warning' },
];

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<WorkOrderApi.WorkOrderItem>,
  onStatusAction?: (code: string, row: WorkOrderApi.WorkOrderItem) => void,
): VxeTableGridOptions<WorkOrderApi.WorkOrderItem>['columns'] {
  return [
    { field: 'code', title: $t('workorder.workorder.code'), width: 150 },
    { field: 'productName', title: $t('workorder.workorder.productName'), width: 150 },
    { field: 'lineName', title: $t('workorder.workorder.lineId'), width: 120 },
    { field: 'processName', title: $t('workorder.workorder.processId'), width: 120 },
    { field: 'plannedQty', title: $t('workorder.workorder.plannedQty'), width: 100 },
    { field: 'completedQty', title: $t('workorder.workorder.completedQty'), width: 110 },
    { field: 'defectQty', title: $t('workorder.workorder.defectQty'), width: 100 },
    {
      cellRender: { name: 'CellTag', options: statusOptions},
      field: 'status',
      title: $t('workorder.workorder.status'),
      width: 100,
    },
    { field: 'plannedStart', title: $t('workorder.workorder.plannedStart'), width: 120 },
    { field: 'plannedEnd', title: $t('workorder.workorder.plannedEnd'), width: 120 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'code', onClick: onActionClick },
        name: 'CellOperation',
        options: ['edit', 'delete'],
        // options: (row: WorkOrderApi.WorkOrderItem) => {
        //   const ops: string[] = [];
        //   const s = row.status;
        //   if (s === 0) { ops.push('edit', 'delete'); }
        //   return ops;
        // },
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('workorder.workorder.operation'),
      width: 200,
    },
    {
      align: 'center',
      field: 'statusAction',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('workorder.workorder.statusAction'),
      width: 200,
      slots: { default: 'statusAction' },
    },
  ];
}
