import type { RouteRecordRaw } from 'vue-router';

import { mergeRouteModules, traverseTreeValues } from '@vben/utils';

import { coreRoutes, fallbackNotFoundRoute } from './core';

// 从模块中导入路由（模块化路由）
// import { aiRoutes } from '#/modules/ai';
// import { mesRoutes } from '#/modules/mes';
// import { rbacRoutes } from '#/modules/rbac';

const dynamicRouteFiles = import.meta.glob('./modules/**/*.ts', {
  eager: true,
});

// 有需要可以自行打开注释，并创建文件夹
// const externalRouteFiles = import.meta.glob('./external/**/*.ts', { eager: true });
// const staticRouteFiles = import.meta.glob('./static/**/*.ts', { eager: true });

/** 动态路由（从 modules/ 目录自动发现，包括 dashboard.ts 等） */
const dynamicRoutes: RouteRecordRaw[] = mergeRouteModules(dynamicRouteFiles);

/** 外部路由列表，访问这些页面可以不需要Layout，可能用于内嵌在别的系统(不会显示在菜单中) */
// const externalRoutes: RouteRecordRaw[] = mergeRouteModules(externalRouteFiles);
// const staticRoutes: RouteRecordRaw[] = mergeRouteModules(staticRouteFiles);
const staticRoutes: RouteRecordRaw[] = [];
const externalRoutes: RouteRecordRaw[] = [];

/** 模块化路由列表（从 modules/{module}/router/ 手动导入） */
const moduleRoutes: RouteRecordRaw[] = [
  //...aiRoutes,
  //...rbacRoutes,
  //...mesRoutes,
];

/** 路由列表，由基本路由、外部路由和404兜底路由组成
 *  无需走权限验证（会一直显示在菜单中） */
const routes: RouteRecordRaw[] = [
  ...coreRoutes,
  ...externalRoutes,
  fallbackNotFoundRoute,
];

/** 基本路由列表，这些路由不需要进入权限拦截 */
const coreRouteNames = traverseTreeValues(coreRoutes, (route) => route.name);

/** 有权限校验的路由列表，包含动态路由、模块化路由和静态路由 */
const accessRoutes = [
  ...dynamicRoutes,
  ...moduleRoutes,
  ...staticRoutes,
];

const componentKeys: string[] = Object.keys(
  import.meta.glob('../../views/**/*.vue'),
)
  .concat(
    Object.keys(import.meta.glob('../../modules/**/views/**/*.vue')),
  )
  .map((v) => {
    const path = v
      .replace('../../views/', '/')
      .replace('../../modules/', '/');
    return path.endsWith('.vue') ? path.slice(0, -4) : path;
  });

export { accessRoutes, componentKeys, coreRouteNames, routes };
