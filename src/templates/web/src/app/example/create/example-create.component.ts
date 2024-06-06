import { Component } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';

@Component({
  selector: 'app-example-create',
  standalone: true,
  imports: [],
  templateUrl: './example-create.component.html',
  styleUrl: './example-create.component.scss'
})
export class ExampleCreateComponent {
  constructor(private http: ExampleHttpClient) {}
}
