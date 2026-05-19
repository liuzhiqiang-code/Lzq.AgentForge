import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { InspectionPlanApi } from '#/modules/equipment/api/inspection';
import { getEquipmentPage } from '#/modules/equipment/api/equipment';

import { $t } from '#/locales';

/**
 * 获取编辑表单字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        // ✅ 直接使用后端接口，自动加载数据
        api: async () => {
          const res = await getEquipmentPage({ pageNum: 1, pageSize: 1000 });
          return (res.items || []).map((item: any) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('equipment.inspectionPlan.equipmentPlaceholder'),
      },
      fieldName: 'equipmentId',
      label: $t('equipment.inspectionPlan.equipmentName'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('equipment.inspectionPlan.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        placeholder: $t('equipment.inspectionPlan.cycleTypePlaceholder'),
        options: [
          { label: $t('equipment.inspectionPlan.cycleTypeDay'), value: 1 },
          { label: $t('equipment.inspectionPlan.cycleTypeWeek'), value: 2 },
          { label: $t('equipment.inspectionPlan.cycleTypeMonth'), value: 3 },
        ],
        class: 'w-full',
      },
      fieldName: 'cycleType',
      label: $t('equipment.inspectionPlan.cycleType'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: {
        placeholder: $t('equipment.inspectionPlan.cycleValuePlaceholder'),
        class: 'w-full',
       },
      fieldName: 'cycleValue',
      label: $t('equipment.inspectionPlan.cycleValue'),
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'nextInspectDate',
      label: $t('equipment.inspectionPlan.nextInspectDate'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        // TODO: 替换为实际的用户 API
        api: async () => {
          // 示例: const res = await getUserPage({ pageNum:1, pageSize:1000 });
          return [{lable:"11",value:"11"}];
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('equipment.inspectionPlan.executorPlaceholder'),
      },
      fieldName: 'executorId',
      label: $t('equipment.inspectionPlan.executorName'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'isEnabled',
      label: $t('equipment.inspectionPlan.isEnabled'),
      defaultValue: 1,
      formItemClass: 'md:col-span-2 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('equipment.inspectionPlan.remark'),
      formItemClass: 'md:col-span-2 col-span-2',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<InspectionPlanApi.InspectionPlanItem>,
): VxeTableGridOptions<InspectionPlanApi.InspectionPlanItem>['columns'] {
  return [
    { field: 'code', title: $t('equipment.inspectionPlan.code'), width: 140 },
    { field: 'name', title: $t('equipment.inspectionPlan.name'), width: 180 },
    { field: 'equipmentName', title: $t('equipment.inspectionPlan.equipmentName'), width: 150 },
    { field: 'cycleTypeName', title: $t('equipment.inspectionPlan.cycleType'), width: 100 },
    { field: 'cycleValue', title: $t('equipment.inspectionPlan.cycleValue'), width: 90 },
    { field: 'nextInspectDate', title: $t('equipment.inspectionPlan.nextInspectDate'), width: 130 },
    { field: 'itemCount', title: $t('equipment.inspectionPlan.itemCount'), width: 90 },
    { field: 'executorName', title: $t('equipment.inspectionPlan.executorName'), width: 100 },
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
      title: $t('equipment.inspectionPlan.operation'),
      width: 160,
    },
  ];
}
