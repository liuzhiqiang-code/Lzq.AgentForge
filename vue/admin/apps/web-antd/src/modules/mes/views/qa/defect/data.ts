import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { DefectApi } from '#/modules/mes/api/defect';
import { getQCOrderPage } from '#/modules/mes/api/qcorder';
import { getWorkOrderPage } from '#/modules/mes/api/workorder';

import { $t } from '#/locales';

/** 处理方式选项 */
const handlingTypeOptions = [
  {label: $t('mes.qa.defect.handlingTypeRework'),value: 1,color: 'processing' },
  {label: $t('mes.qa.defect.handlingTypeScrap'),value: 2,color: 'processing' },
  {label: $t('mes.qa.defect.handlingTypeDowngrade'),value: 3,color: 'processing' },
  {label: $t('mes.qa.defect.handlingTypeReturn'),value: 4,color: 'processing' },
  {label: $t('mes.qa.defect.handlingTypeAcceptSpecial'),value: 5,color: 'processing' },
];

/** 状态映射 */
const statusOptions = [
  {label: $t('mes.qa.defect.statusPending'),value: '0',color: 'processing' },
  {label: $t('mes.qa.defect.statusProcessing'),value: '1',color: 'processing' },
  {label: $t('mes.qa.defect.statusProcessed'),value: '2',color: 'processing' },
];

/**
 * 获取编辑表单字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        api: async () => {
          const res = await getQCOrderPage({ pageNum: 1, pageSize: 1000 });
          return (res.items || []).map((item: any) => ({
            label: `${item.code} - ${item.productName || ''}`,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('mes.qa.defect.qcOrderPlaceholder'),
      },
      fieldName: 'qcOrderId',
      label: $t('mes.qa.defect.qcOrderCode'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        // ? 直接使用后端接口
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
        placeholder: $t('mes.qa.defect.workOrderPlaceholder'),
      },
      fieldName: 'workOrderId',
      label: $t('mes.qa.defect.workOrderCode'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'productName',
      label: $t('mes.qa.defect.productName'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'batchNo',
      label: $t('mes.qa.defect.batchNo'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'defectQty',
      label: $t('mes.qa.defect.defectQty'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'defectCode',
      label: $t('mes.qa.defect.defectCode'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'defectDesc',
      label: $t('mes.qa.defect.defectDesc'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'defectImages',
      label: $t('mes.qa.defect.defectImages'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: handlingTypeOptions.map(a => ({ label: a.label, value: Number(a.value) })),
        placeholder: '请选择处理方式',
        class: 'w-full',
      },
      fieldName: 'handlingType',
      label: $t('mes.qa.defect.handlingType'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('mes.qa.defect.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<DefectApi.DefectItem>,
  onStatusAction?: (code: string, row: DefectApi.DefectItem) => void,
): VxeTableGridOptions<DefectApi.DefectItem>['columns'] {
  return [
    { field: 'qcOrderCode', title: $t('mes.qa.defect.qcOrderCode'), width: 140 },
    { field: 'workOrderCode', title: $t('mes.qa.defect.workOrderCode'), width: 140 },
    { field: 'productName', title: $t('mes.qa.defect.productName'), width: 140 },
    { field: 'batchNo', title: $t('mes.qa.defect.batchNo'), width: 100 },
    { field: 'defectQty', title: $t('mes.qa.defect.defectQty'), width: 90 },
    { field: 'defectCode', title: $t('mes.qa.defect.defectCode'), width: 120 },
    { field: 'defectDesc', title: $t('mes.qa.defect.defectDesc'), minWidth: 180 },
    {
      cellRender: { name: 'CellTag',options: statusOptions },
      field: 'status',
      title: $t('mes.qa.defect.status'),
      width: 100,
    },
    {
      cellRender: { name: 'CellTag', options: handlingTypeOptions },
      field: 'handlingType',
      title: $t('mes.qa.defect.handlingType'),
      width: 120,
    },
    { field: 'handlerName', title: $t('mes.qa.defect.handlerName'), width: 100 },
    { field: 'handlingTime', title: $t('mes.qa.defect.handlingTime'), width: 120 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'defectCode', onClick: onActionClick },
        name: 'CellOperation',
        options:['edit', 'delete'],
        // options: (row: DefectApi.DefectItem) => {
        //   const ops: string[] = [];
        //   if (row.status === 0) { ops.push('edit', 'delete'); }
        //   return ops;
        // },
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('mes.qa.defect.operation'),
      width: 160,
    },
    {
      align: 'center',
      field: 'statusAction',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('mes.qa.defect.statusAction'),
      width: 160,
      slots: { default: 'statusAction' },
    },
  ];
}
