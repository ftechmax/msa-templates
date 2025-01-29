import { Component, Input } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';

@Component({
    selector: 'app-example-update',
    imports: [],
    providers: [ExampleHttpClient],
    templateUrl: './example-update.component.html',
    styleUrl: './example-update.component.scss'
})
export class ExampleUpdateComponent {
  @Input() id: string = '';

  constructor(private http: ExampleHttpClient) {}
}
