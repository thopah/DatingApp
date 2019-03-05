import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.css']
})
export class ValueComponent implements OnInit {

  values: any;
  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.getValues();
  }

  getValues() {
    return this.httpClient.get('http://localhost:5000/api/values').subscribe(response => {
      this.values = response;
    },
    error => {
      console.error(error);
    }
    );
  }
}
