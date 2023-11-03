import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ExampleCollectionDto, ExampleDetailsDto } from './example-contracts';

const endpoint: string = 'examples';

@Injectable()
export class ExampleHttpClient {
  constructor(
    private http: HttpClient,
    @Inject('BASE_API_URL') private baseUrl: string
  ) {}

  getCollection() {
    return this.http.get<ExampleCollectionDto[]>(this.getUrl());
  }

  getDetails(id: string) {
    return this.http.get<ExampleDetailsDto>(this.getUrl(id));
  }

  create(data: any) {
    return this.http.post(this.getUrl(), data);
  }

  update(id: string, data: any) {
    return this.http.put(this.getUrl(id), data);
  }

  private getUrl(url: string = '') {
    return this.baseUrl + `/api/${endpoint}/` + url;
  }
}
