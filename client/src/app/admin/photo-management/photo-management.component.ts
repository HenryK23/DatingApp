import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/Photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  user: any[];
  unmoderatedPhotos: any[] = [];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getUnmoderatedPhotos();
  }

  getUnmoderatedPhotos(){
    this.adminService.getUnmoderatedPhotos().subscribe(photos => {
      this.user = photos;
      this.user.forEach(element => {
        element.unmoderatedPhotos.forEach(element => {
          this.unmoderatedPhotos.push(element);
        });
      });
    })
  }

  approvePhoto(userId: string, photoId: string){
    this.adminService.approvePhoto(userId, photoId).subscribe(() => {
      this.unmoderatedPhotos = [];
      this.getUnmoderatedPhotos();
    })
  }
  disapprovePhoto(userId:string, photoId:string){
    this.adminService.disapprovePhoto(userId, photoId).subscribe(() =>{
      this.unmoderatedPhotos = [];
      this.getUnmoderatedPhotos();
    })
  }
}
