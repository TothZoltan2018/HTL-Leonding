import { HttpClient } from '@angular/common/http';
import { Component, VERSION, OnInit } from '@angular/core';

export interface GameGenre {
  id: number;
  name: string;
}

export interface Game {
  id: number;
  name: string;
  genre: GameGenre;
  personalRating: number;
}

@Component({
  selector: 'my-app',
  templateUrl: './app.component.html',
  styleUrls: [ './app.component.css' ]
})
export class AppComponent implements OnInit {
  public games: Game[] = [];

  constructor(private client: HttpClient) {} // constructor injection

  // A method to use data get from Web API
  ngOnInit() {
    this.client.get<Game[]>('https://localhost:5001/api/games')
    .subscribe(result => this.games = result);
  }
}
