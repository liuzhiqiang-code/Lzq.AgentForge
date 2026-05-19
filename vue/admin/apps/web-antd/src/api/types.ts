export interface SelectViewDto {
  [key: string]: any;
  label: string;
  value: string;
}

export interface PagedResponse<T> {
  [key: string]: any;
  items: Array<T>;
  total: number;
}
