import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { MaterialApi } from '#/modules/mes/api/material';

import { $t } from '#/locales';
import { h } from 'vue';
import { Tag } from 'ant-design-vue';

export function useSearchSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'code',
      label: $t('mes.basadata.material.code'),
      componentProps: { placeholder: $t('mes.basadata.material.code') },
    },
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('mes.basadata.material.name'),
      componentProps: { placeholder: $t('mes.basadata.material.name') },
    },
    {
      component: 'ApiSelect',
      fieldName: 'materialTypeId',
      label: $t('mes.basadata.material.materialType'),
      componentProps: {
        placeholder: $t('mes.basadata.material.materialType'),
        api: async () => {
          const { getMaterialTypeTree } = await import('#/modules/mes/api/material');
          const tree = await getMaterialTypeTree();
          const flatten = (items: MaterialApi.MaterialTypeItem[], prefix = ''): any[] => {
            const result: any[] = [];
            for (const item of items) {
              result.push({ label: prefix + item.name, value: item.id });
              if (item.children?.length) {
                result.push(...flatten(item.children, prefix + '── '));
              }
            }
            return result;
          };
          return flatten(tree);
        },
      },
    },
    {
      component: 'Select',
      fieldName: 'status',
      label: $t('mes.basadata.material.status'),
      componentProps: {
        placeholder: $t('mes.basadata.material.status'),
        options: [
          { label: $t('mes.basadata.material.materialStatusEnabled'), value: 0 },
          { label: $t('mes.basadata.material.materialStatusDisabled'), value: 1 },
          { label: $t('mes.basadata.material.materialStatusObsolete'), value: 2 },
        ],
        allowClear: true,
      },
    },
  ];
}

export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('mes.basadata.material.code'),
      rules: 'required',
      formItemClass: 'col-span-1',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('mes.basadata.material.name'),
      rules: 'required',
      formItemClass: 'col-span-1',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'spec',
      label: $t('mes.basadata.material.spec'),
      formItemClass: 'col-span-1',
    },
    {
      component: 'ApiSelect',
      fieldName: 'materialTypeId',
      label: $t('mes.basadata.material.materialType'),
      rules: 'required',
      componentProps: {
        placeholder: $t('mes.basadata.material.materialType'),
        api: async () => {
          const { getMaterialTypeTree } = await import('#/modules/mes/api/material');
          const tree = await getMaterialTypeTree();
          const flatten = (items: MaterialApi.MaterialTypeItem[], prefix = ''): any[] => {
            const result: any[] = [];
            for (const item of items) {
              result.push({ label: prefix + item.name, value: item.id });
              if (item.children?.length) {
                result.push(...flatten(item.children, prefix + '── '));
              }
            }
            return result;
          };
          return flatten(tree);
        },
      },
      formItemClass: 'col-span-1',
    },
    {
      component: 'ApiSelect',
      fieldName: 'unitId',
      label: $t('mes.basadata.material.unit'),
      rules: 'required',
      componentProps: {
        placeholder: $t('mes.basadata.material.unit'),
        api: async () => {
          const { getUnitOfMeasureSelectList } = await import('#/modules/mes/api/material');
          const list = await getUnitOfMeasureSelectList();
          return (list || []).map((u: any) => ({ label: u.name, value: u.id }));
        },
      },
      formItemClass: 'col-span-1',
    },
    {
      component: 'InputNumber',
      fieldName: 'weight',
      label: $t('mes.basadata.material.weight'),
      componentProps: { min: 0, precision: 4 },
      formItemClass: 'col-span-1',
    },
    {
      component: 'InputNumber',
      fieldName: 'volume',
      label: $t('mes.basadata.material.volume'),
      componentProps: { min: 0, precision: 4 },
      formItemClass: 'col-span-1',
    },
    {
      component: 'Select',
      fieldName: 'status',
      label: $t('mes.basadata.material.status'),
      componentProps: {
        options: [
          { label: $t('mes.basadata.material.materialStatusEnabled'), value: 0 },
          { label: $t('mes.basadata.material.materialStatusDisabled'), value: 1 },
          { label: $t('mes.basadata.material.materialStatusObsolete'), value: 2 },
        ],
      },
      defaultValue: 0,
      formItemClass: 'col-span-2',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<MaterialApi.MaterialItem>,
): VxeTableGridOptions<MaterialApi.MaterialItem>['columns'] {
  return [
    { field: 'code', title: $t('mes.basadata.material.code'), width: 130 },
    { field: 'name', title: $t('mes.basadata.material.name'), width: 160, minWidth: 120 },
    { field: 'spec', title: $t('mes.basadata.material.spec'), width: 140, showOverflow: true },
    { field: 'materialTypeName', title: $t('mes.basadata.material.materialType'), width: 110 },
    { field: 'unitName', title: $t('mes.basadata.material.unit'), width: 90 },
    {
      field: 'status',
      title: $t('mes.basadata.material.status'),
      width: 90,
      cellRender: {
        name: 'CellTag',
        attrs: {
          options: [
            { color: 'success', label: $t('mes.basadata.material.materialStatusEnabled'), value: 0 },
            { color: 'error', label: $t('mes.basadata.material.materialStatusDisabled'), value: 1 },
            { color: 'warning', label: $t('mes.basadata.material.materialStatusObsolete'), value: 2 },
          ],
        },
      },
    },
    { field: 'creationTime', title: $t('common.createdAt'), width: 170 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'name', onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
          { code: 'bom', text: $t('mes.basadata.material.bomManage') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('mes.basadata.material.operation'),
      width: 250,
    },
  ];
}
