import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { map, Observable, of } from 'rxjs';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})

export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];


  constructor(private http:HttpClient) { }
  getMembers() {
    if (this.members.length > 0) return of(this.members);
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        return members;
      })
    )
  }

  getMember(username: string) {
    const member = this.members.find(x => x.username === username);
    if (member) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member:Member){
    return this.http.put<Member>(this.baseUrl + 'users',member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
      })
    )
  }
}
