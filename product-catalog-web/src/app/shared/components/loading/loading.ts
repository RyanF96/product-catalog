import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading',
  imports: [],
  templateUrl: './loading.html'
})
export class Loading {
  readonly visible = input<boolean>(true);
  readonly size = input<number>(40);
  readonly message = input<string>('');
  readonly containerClass = input<string>('');
}
