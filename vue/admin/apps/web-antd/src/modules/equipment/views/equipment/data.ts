import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { EquipmentApi } from '#/modules/equipment/api/equipment';
import { getLineListByWorkshop } from '#/modules/basadata/api/line';
import { getWorkshopPage } from '#/modules/basadata/api/workshop';

import { $t } from '#/locales';

const equipmentTypeOptions: Record<number, string> = {
  1: $t('equipment.equipment.equipmentTypeProduction'),
  2: $t('equipment.equipment.equipmentTypeTesting'),
  3: $t('equipment.equipment.equipmentTypeAuxiliary'),
  4: $t('equipment.equipment.equipmentTypePower'),
  5: $t('equipment.equipment.equipmentTypeTransport'),
};

const statusMap: Record<number, string> = {
  0: $t('equipment.equipment.statusNormal'),
  1: $t('equipment.equipment.statusUnderRepair'),
  2: $t('equipment.equipment.statusUnderMaintenance'),
  3: $t('equipment.equipment.statusStopped'),
  4: $t('equipment.equipment.statusScrapped'),
};

export function useSchema(): VbenFormSchema[] {
  return [
    // 1. 车间选择（用 trigger 清空产线）
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        showSearch: true,
        placeholder: '请选择车间',
        class: 'w-full',
        api: async () => {
          const res = await getWorkshopPage({ page: 1, pageSize: 999 });
          return (res?.items || []).map((item) => ({
            label: item.name,
            value: item.id,
          }));
        },
      },
      fieldName: 'workshopId',
      label: '车间',
      rules: 'required',
      // 新增 dependencies，当车间值变化时清空产线
      dependencies: {
        trigger(values, form) {
          form.setFieldValue('lineId', undefined);
        },
        triggerFields: ['workshopId'],
      },
      formItemClass: 'md:col-span-1 col-span-2',
    },
    // 2. 产线下拉（componentProps 动态获取车间 ID）
    {
      component: 'ApiSelect',
      fieldName: 'lineId',
      label: '产线',
      dependencies: {
        // 动态修改组件参数，workshopId 变化时重新执行
        componentProps(values) {
          return {
            allowClear: true,
            showSearch: true,
            placeholder: '请先选择车间',
            class: 'w-full',
            params: { workshopId: values.workshopId },
            api: async ({ workshopId }) => {
              if (!workshopId) return [];
              const res = await getLineListByWorkshop(workshopId);
              return (res || []).map((item) => ({
                label: `${item.code} - ${item.name}`,
                value: item.id,
              }));
            },
          };
        },
        triggerFields: ['workshopId'],
      },
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('equipment.equipment.code'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('equipment.equipment.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        allowClear: false,
        options: Object.entries(equipmentTypeOptions).map(([k, v]) => ({ label: v, value: Number(k) })),
        placeholder: '请选择设备类型',
      },
      fieldName: 'equipmentType',
      label: $t('equipment.equipment.equipmentType'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'spec',
      label: $t('equipment.equipment.spec'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'brand',
      label: $t('equipment.equipment.brand'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'supplier',
      label: $t('equipment.equipment.supplier'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'purchaseDate',
      label: $t('equipment.equipment.purchaseDate'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'warrantyEndDate',
      label: $t('equipment.equipment.warrantyEndDate'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'location',
      label: $t('equipment.equipment.location'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('equipment.equipment.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<EquipmentApi.EquipmentItem>,
): VxeTableGridOptions<EquipmentApi.EquipmentItem>['columns'] {
  return [
    { field: 'code', title: $t('equipment.equipment.code'), width: 120 },
    { field: 'name', title: $t('equipment.equipment.name'), width: 150 },
    {
      cellRender: { name: 'CellTag', attrs: { options: equipmentTypeOptions } },
      field: 'equipmentType',
      title: $t('equipment.equipment.equipmentType'),
      width: 110,
    },
    { field: 'spec', title: $t('equipment.equipment.spec'), width: 120 },
    { field: 'brand', title: $t('equipment.equipment.brand'), width: 100 },
    { field: 'lineName', title: $t('equipment.equipment.lineName'), width: 100 },
    { field: 'location', title: $t('equipment.equipment.location'), width: 120 },
    {
      cellRender: { name: 'CellTag', attrs: { options: statusMap } },
      field: 'status',
      title: $t('equipment.equipment.status'),
      width: 100,
    },
    { field: 'responsibleName', title: $t('equipment.equipment.responsibleName'), width: 100 },
    { field: 'purchaseDate', title: $t('equipment.equipment.purchaseDate'), width: 110 },
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
      title: $t('equipment.equipment.operation'),
      width: 160,
    },
  ];
}

export { equipmentTypeOptions, statusMap };
