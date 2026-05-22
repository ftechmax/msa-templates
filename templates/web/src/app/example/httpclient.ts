import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ExampleCollectionDto, ExampleDetailsDto } from './contracts';

@Injectable()
export class ExampleHttpClient {
  constructor(private http: HttpClient) {}

  getCollection() {
    return this.http.get<ExampleCollectionDto[]>(this.getUrl());
  }

  getCollectionPaged(
    sort: string,
    order: string,
    page: number,
    pageSize: number
  ) {
    return this.http.get<ExampleCollectionDto[]>(this.getUrl());
  }

  getDetails(id: string) {
    return this.http.get<ExampleDetailsDto>(this.getUrl(`${id}`));
  }

  create(data: any) {
    return this.http.post(this.getUrl(), data);
  }

  update(id: string, data: any) {
    return this.http.put(this.getUrl(`${id}`), data);
  }

  private getUrl(url: string = '') {
    return '/api/example/' + url;
  }
}
