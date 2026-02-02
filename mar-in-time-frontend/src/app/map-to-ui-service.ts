import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({ providedIn: 'root' })
export class MapToUiService {

    private detailsSource = new Subject<any>();
    details$ = this.detailsSource.asObservable();

    send(details: any) {
        this.detailsSource.next(details);
    }
}