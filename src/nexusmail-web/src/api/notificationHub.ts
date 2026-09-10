import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface NotificationHubCallbacks {
  onReceiveNotification: (notification: any) => void;
  onReconnecting?: () => void;
  onReconnected?: () => void;
  onClose?: (error?: Error) => void;
}

export class NotificationHubService {
  private connection: HubConnection | null = null;
  private readonly hubUrl = 'http://localhost:5038/hub/notifications';

  public async connect(
    accessToken: string,
    workspaceId: string,
    callbacks: NotificationHubCallbacks
  ): Promise<void> {
    if (this.connection) {
      await this.disconnect();
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}?workspaceId=${workspaceId}`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.on('ReceiveNotification', callbacks.onReceiveNotification);

    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting...', error);
      if (callbacks.onReconnecting) callbacks.onReconnecting();
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected. Connection ID:', connectionId);
      if (callbacks.onReconnected) callbacks.onReconnected();
    });

    this.connection.onclose((error) => {
      console.warn('SignalR connection closed.', error);
      if (callbacks.onClose) callbacks.onClose(error);
    });

    try {
      await this.connection.start();
      console.log('SignalR connected to NotificationHub.');
    } catch (err) {
      console.error('SignalR connection failed:', err);
      throw err;
    }
  }

  public async disconnect(): Promise<void> {
    if (this.connection) {
      this.connection.off('ReceiveNotification');
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export const notificationHub = new NotificationHubService();
