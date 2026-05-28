import { baseRequestClient, requestClient } from '#/api/request';

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
    userName?: string;
  }

    /** 注册接口参数 */
  export interface RegisterParams {
    userName?: string;
    password?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    access_token: string;
    refreshToken: string;
  }

  export interface RefreshTokenResult {
    data: string;
    status: number;
  }

  export interface RefreshTokenParams {
    refreshToken: null | string;
  }
}

/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  data.userName = data.username;
  return await requestClient.post<AuthApi.LoginResult>('/rbac/account/login', data);
}

/**
 * 注册
 */
export async function registerApi(data: AuthApi.RegisterParams) {
  return await requestClient.post('/rbac/account/register', data);
}

/**
 * 刷新accessToken
 */
export async function refreshTokenApi(data: AuthApi.RefreshTokenParams) {
  return baseRequestClient.post<AuthApi.LoginResult>('/rbac/account/refresh', data);
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return requestClient.post('/rbac/account/logout', {
    withCredentials: true,
  });
}

/**
 * 获取用户权限码
 */
export async function getAccessCodesApi() {
  return requestClient.get<string[]>('/rbac/menu/accessCodes');
}
