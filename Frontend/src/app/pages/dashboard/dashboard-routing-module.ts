import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { LayoutComponent } from './layout/layout';
import { TicketListComponent } from './tickets/ticket-list/ticket.list';
import { TicketDetailComponent } from './tickets/ticket-detail/ticket-detail';
import { TicketCreateComponent } from './tickets/ticket-create/ticket-create';

// Dashboard Routes
const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],

    children: [
      { path: '', redirectTo: 'my-tickets', pathMatch: 'full' },
      { path: 'my-tickets', component: TicketListComponent },
      { path: 'create-ticket', component: TicketCreateComponent },
      { path: 'all-tickets', component: TicketListComponent },
      { path: 'ticket/:id', component: TicketDetailComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }