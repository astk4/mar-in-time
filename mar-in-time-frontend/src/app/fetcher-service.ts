import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FetcherService {

  constructor(private http: HttpClient) {}

  getItem(apiUrl: string): Observable<any> {
    return this.http.get<any>(apiUrl);
  }
}