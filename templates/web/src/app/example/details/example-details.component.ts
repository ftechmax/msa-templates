import { Component, Input } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-example-details',
    imports: [RouterLink],
    providers: [ExampleHttpClient],
    templateUrl: './example-details.component.html',
    styleUrl: './example-details.component.scss'
})
export class ExampleDetailsComponent {
  @Input() id: string = '';

  constructor(private http: ExampleHttpClient) {}
}
