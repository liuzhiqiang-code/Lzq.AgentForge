import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { InspectionRecordApi } from '#/modules/equipment/api/inspection';

import { $t } from '#/locales';

const resultMap: Record<number, string> = {
  0: $t('equipment.inspectionRecord.resultNormal'),
  1: $t('equipment.inspectionRecord.resultAbnormal'),
  2: $t('equipment.inspectionRecord.resultNeedRepair'),
};

export function useColumns(): VxeTableGridOptions<InspectionRecordApi.InspectionRecordItem>['columns'] {
  return [
    { field: 'code', title: $t('equipment.inspectionRecord.code'), width: 150 },
    { field: 'equipmentName', title: $t('equipment.inspectionRecord.equipmentName'), width: 150 },
    { field: 'inspectDate', title: $t('equipment.inspectionRecord.inspectDate'), width: 130 },
    {
      cellRender: { name: 'CellTag', attrs: { options: resultMap } },
      field: 'result',
      title: $t('equipment.inspectionRecord.result'),
      width: 100,
    },
    { field: 'inspectorName', title: $t('equipment.inspectionRecord.inspectorName'), width: 100 },
    { field: 'completedTime', title: $t('equipment.inspectionRecord.completedTime'), width: 130 },
    { field: 'durationMinutes', title: $t('equipment.inspectionRecord.durationMinutes'), width: 100 },
    { field: 'abnormalDesc', title: $t('equipment.inspectionRecord.abnormalDesc'), minWidth: 150 },
    { field: 'remark', title: $t('equipment.inspectionRecord.remark'), width: 150 },
  ];
}

export { resultMap };
