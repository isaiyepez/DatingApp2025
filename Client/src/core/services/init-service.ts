import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  //Add provideAppInitilizer in app.config
  init() {
    const userString = localStorage.getItem('user');

    if(!userString) return of(null);

    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
    //Meaning "observable of"
    return of(null);
  }
}
