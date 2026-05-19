import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:view-dashboard-outline',
      order: 1040,
      title: $t('dashboard.title'),
    },
    name: 'MESDashboard',  // 改为 MESDashboard，避免与 Playground 的 Dashboard 冲突
    path: '/mes-dashboard',
    children: [
      {
        path: '/dashboard/index',
        name: 'DashboardIndex',
        meta: {
          icon: 'mdi:monitor-dashboard',
          title: $t('dashboard.overview'),
        },
        component: () => import('#/modules/dashboard/views/index.vue'),
      },
    ],
  },
];

export default routes;
