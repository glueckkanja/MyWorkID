import { Avatar, useTheme } from "@mui/material";
import PlaceholderImage from "../../assets/img/placeholder.png";
import { useEffect, useState } from "react";
import { getUser, getUserImage } from "../../services/ApiService";
import { User } from "../../types";

export const UserDisplay = () => {
  const theme = useTheme();
  const [user, setUser] = useState<User>();
  const [userImage, setUserImage] = useState<string>();

  useEffect(() => {
    getUser().then((usr) => {
      setUser(usr);
    });
    getUserImage()
      .then((imgBlob) => {
        var reader = new FileReader();
        reader.readAsDataURL(imgBlob);
        reader.onloadend = () => {
          var base64String = reader.result?.toString();
          setUserImage(base64String);
        };
      })
      .catch(() => {
        // Ignore - this is thrown if no image is set
      });
  }, []);

  return (
    <div className="user_display">
      <Avatar sx={{ width: 100, height: 100 }} src={userImage}></Avatar>
      <div
        className="user_display__user_info__container"
      >
        <div>
          <h3 className="no_margin">{user?.displayName}</h3>
          <div>
            Risk State:{" "}
            <span style={{ color: theme.palette.success.main }}>Low</span>
          </div>
        </div>
      </div>
    </div>
  );
};
