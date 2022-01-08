import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles(){
    return this.http.get<Partial<User[]>>(this.baseUrl + "admin/users-with-roles");
  }

  updateUserRoles(username: string, roles: string[]){
    return this.http.post(this.baseUrl + "admin/edit-roles/" + username + "?roles=" + roles, {});

  }

  getUnmoderatedPhotos(){
    return this.http.get<any[]>(this.baseUrl + "admin/photos-to-moderate");
  }

  approvePhoto(userId: string, photoId: string){
    return this.http.put(this.baseUrl + "admin/approve-photo/"+userId+"?photoId="+photoId, {});
  }

  disapprovePhoto(userId: string, photoId: string){
    console.log("disapproved user id: " + userId + " photoid: " + photoId);
    return this.http.delete(this.baseUrl + "admin/disapprove-photo/"+userId+"?photoId="+photoId, {});
    
  }
}
