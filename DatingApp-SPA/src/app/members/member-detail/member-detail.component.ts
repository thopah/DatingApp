import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { User } from 'src/app/_models/user';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation} from 'ngx-gallery';
@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  constructor(private userService: UserService, private route: ActivatedRoute, private alertify: AlertifyService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
    });
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

    this.galleryImages = this.getImages();
  }

  getImages() {
    const imageUrls = [];
    for (let i = 0; i < this.user.photos.length; i++) {
      imageUrls.push({
        small: 'assets/_images/mark.png',
        medium: 'assets/_images/mark.png',
        big: 'assets/_images/mark.png',
      });
    }
    return imageUrls;
  }
}
