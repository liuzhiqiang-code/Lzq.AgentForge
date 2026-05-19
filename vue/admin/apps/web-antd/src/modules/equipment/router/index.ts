import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:tools',
      order: 1030,
      title: $t('equipment.title'),
    },
    name: 'Equipment',
    path: '/equipment',
    children: [
      {
        path: '/equipment/list',
        name: 'EquipmentList',
        meta: {
          icon: 'mdi:excavator',
          title: $t('equipment.equipment.title'),
        },
        component: () => import('#/modules/equipment/views/equipment/list.vue'),
      },
      {
        path: '/equipment/inspection-plan',
        name: 'InspectionPlan',
        meta: {
          icon: 'mdi:clipboard-text-search-outline',
          title: $t('equipment.inspectionPlan.title'),
        },
        component: () => import('#/modules/equipment/views/inspection/plan/list.vue'),
      },
      {
        path: '/equipment/inspection-record',
        name: 'InspectionRecord',
        meta: {
          icon: 'mdi:clipboard-check-outline',
          title: $t('equipment.inspectionRecord.title'),
        },
        component: () => import('#/modules/equipment/views/inspection/record/list.vue'),
      },
      {
        path: '/equipment/maintenance-plan',
        name: 'MaintenancePlan',
        meta: {
          icon: 'mdi:wrench-clock-outline',
          title: $t('equipment.maintenancePlan.title'),
        },
        component: () => import('#/modules/equipment/views/maintenance/plan/list.vue'),
      },
      {
        path: '/equipment/maintenance-record',
        name: 'MaintenanceRecord',
        meta: {
          icon: 'mdi:wrench-check-outline',
          title: $t('equipment.maintenanceRecord.title'),
        },
        component: () => import('#/modules/equipment/views/maintenance/record/list.vue'),
      },
      {
        path: '/equipment/repair',
        name: 'Repair',
        meta: {
          icon: 'mdi:hammer-wrench',
          title: $t('equipment.repair.title'),
        },
        component: () => import('#/modules/equipment/views/repair/list.vue'),
      },
    ],
  },
];

export default routes;
