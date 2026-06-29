import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  private statusUpdateSubject = new Subject<{ id: string, status: string }>();
  private reconnectedSubject = new Subject<void>();

  constructor(private zone: NgZone) { }

  public startConnection = () => {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = localStorage.getItem('token');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7163/podcastHub', {
        accessTokenFactory: () => token ? token : ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onreconnected(() => {
      console.log('SignalR: Reconnected ✅');
      this.zone.run(() => {
        this.reconnectedSubject.next();
      });
    });

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR: Connection started ✨');
        this.registerStatusChangedListener();
      })
      .catch(err => console.error('SignalR: Error while starting connection: ', err));
  }

  private registerStatusChangedListener = () => {
    this.hubConnection.on('ReceiveStatusUpdate', (data: { podcastId: string, status: string }) => {
      console.log('📡 SignalR Received:', data);
      this.zone.run(() => {
        this.statusUpdateSubject.next({
          id: data.podcastId,
          status: data.status
        });
      });
    });
  }

  public onStatusChanged(): Observable<{ id: string, status: string }> {
    return this.statusUpdateSubject.asObservable();
  }

  public onReconnected(): Observable<void> {
    return this.reconnectedSubject.asObservable();
  }

  public stopConnection = () => {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR: Connection stopped 🛑'))
        .catch(err => console.error('SignalR: Error while stopping: ', err));
    }
  }
}