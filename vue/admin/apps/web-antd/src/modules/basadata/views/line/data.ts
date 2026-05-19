import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { LineApi } from '#/modules/basadata/api/line';

import { getWorkshopPage } from '#/modules/basadata/api/workshop';
import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: async () => {
          const res = await getWorkshopPage({ page: 1, pageSize: 999 });
          return (res?.items || []).map((item) => ({ label: item.name, value: item.id }));
        },
        placeholder: '请选择车间',
        showSearch: true,
      },
      fieldName: 'workshopId',
      label: $t('basadata.line.workshopId'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('basadata.line.code'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('basadata.line.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'status',
      label: $t('basadata.line.status'),
      defaultValue: 1,
      formItemClass: 'col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<LineApi.LineItem>,
): VxeTableGridOptions<LineApi.LineItem>['columns'] {
  return [
    { field: 'code', title: $t('basadata.line.code') },
    { field: 'name', title: $t('basadata.line.name') },
    { field: 'workshopName', title: $t('basadata.line.workshopId') },
    {
      cellRender: { name: 'CellTag', attrs: { options: { 0: '禁用', 1: '启用' } } },
      field: 'status',
      title: $t('basadata.line.status'),
    },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'name', onClick: onActionClick },
        name: 'CellOperation',
        options: ['edit', 'delete'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('basadata.line.operation'),
      width: 180,
    },
  ];
}
