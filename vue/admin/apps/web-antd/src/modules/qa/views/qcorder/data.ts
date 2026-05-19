import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { QCOrderApi } from '#/modules/qa/api/qcorder';

import { $t } from '#/locales';

/** QC 类型选项 */
const qcTypeOptions = [
  { value: 1, label: $t('qa.qcorder.qcTypeIQC'), color: 'processing' },
  { value: 2, label: $t('qa.qcorder.qcTypePQC'), color: 'processing' },
  { value: 3, label: $t('qa.qcorder.qcTypeOQC'), color: 'processing' },
];

/** 状态映射 */
const statusOptions = [
  { value: 0, label: $t('qa.qcorder.statusPending'), color: 'processing' },
  { value: 1, label: $t('qa.qcorder.statusInProgress'), color: 'processing' },
  { value: 2, label: $t('qa.qcorder.statusQualified'), color: 'processing' },
  { value: 3, label: $t('qa.qcorder.statusUnqualified'), color: 'processing' },
  { value: 4, label: $t('qa.qcorder.statusProcessed'), color: 'processing' },
  { value: 5, label: $t('qa.qcorder.statusCancelled'), color: 'processing' },
];

/**
 * 获取编辑表单字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        allowClear: false,
        options: Object.entries(qcTypeOptions).map(([k, v]) => ({ label: v, value: Number(k) })),
        //placeholder: $t('qa.qcorder.qcTypePlaceholder'),
      },
      fieldName: 'qcType',
      label: $t('qa.qcorder.qcType'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'refCode',
      label: $t('qa.qcorder.refCode'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'productName',
      label: $t('qa.qcorder.productName'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'productSpec',
      label: $t('qa.qcorder.productSpec'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'batchNo',
      label: $t('qa.qcorder.batchNo'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'supplierName',
      label: $t('qa.qcorder.supplierName'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full' },
      fieldName: 'submitQty',
      label: $t('qa.qcorder.submitQty'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'qcStandard',
      label: $t('qa.qcorder.qcStandard'),
      formItemClass: 'md:col-span-2 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('qa.qcorder.remark'),
      formItemClass: 'md:col-span-2 col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<QCOrderApi.QCOrderItem>,
  onStatusAction?: (code: string, row: QCOrderApi.QCOrderItem) => void,
): VxeTableGridOptions<QCOrderApi.QCOrderItem>['columns'] {
  return [
    { field: 'code', title: $t('qa.qcorder.code'), width: 150 },
    {
      cellRender: { name: 'CellTag', options: qcTypeOptions },
      field: 'qcType',
      title: $t('qa.qcorder.qcType'),
      width: 100,
    },
    { field: 'productName', title: $t('qa.qcorder.productName'), width: 150 },
    { field: 'productSpec', title: $t('qa.qcorder.productSpec'), width: 120 },
    { field: 'batchNo', title: $t('qa.qcorder.batchNo'), width: 120 },
    { field: 'supplierName', title: $t('qa.qcorder.supplierName'), width: 120 },
    { field: 'submitQty', title: $t('qa.qcorder.submitQty'), width: 90 },
    { field: 'qualifiedQty', title: $t('qa.qcorder.qualifiedQty'), width: 90 },
    { field: 'unqualifiedQty', title: $t('qa.qcorder.unqualifiedQty'), width: 100 },
    {
      cellRender: { name: 'CellTag', options: statusOptions },
      field: 'status',
      title: $t('qa.qcorder.status'),
      width: 100,
    },
    { field: 'inspectorName', title: $t('qa.qcorder.inspectorName'), width: 100 },
    { field: 'inspectDate', title: $t('qa.qcorder.inspectDate'), width: 120 },
    { field: 'conclusion', title: $t('qa.qcorder.conclusion'), width: 100 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'code', onClick: onActionClick },
        name: 'CellOperation',
        options: [
          {
            code: 'edit',
            text: '编辑',
            disabled: (row: QCOrderApi.QCOrderItem) => row.status === 0,
          },
          {
            code: 'delete',
            text: '删除',
            disabled: (row: QCOrderApi.QCOrderItem) => row.status === 0,
          }
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('qa.qcorder.operation'),
      width: 160,
    },
    {
      align: 'center',
      field: 'statusAction',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('qa.qcorder.statusAction'),
      width: 200,
      slots: { default: 'statusAction' },
    },
  ];
}
