import { Component } from '@angular/core';

@Component({
  selector: 'app-layout',
  standalone: false,
  template: `
    <div class="flex h-screen overflow-hidden bg-slate-50 dark:bg-background-dark">
      <app-sidebar></app-sidebar>
      
      <div class="flex-1 flex flex-col overflow-hidden">
        <app-topbar></app-topbar>
        
        <main class="flex-1 overflow-y-auto p-8">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class LayoutComponent {}