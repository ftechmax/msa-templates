import { Component, Input } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';

@Component({
  selector: 'app-example-details',
  standalone: true,
  imports: [],
  templateUrl: './example-details.component.html',
  styleUrl: './example-details.component.scss'
})
export class ExampleDetailsComponent {
  @Input() id : string = '';

  constructor(private http: ExampleHttpClient) {}
}
