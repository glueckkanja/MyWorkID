import React from "react";
import { authenticateRequest } from "../services/MsalService";
import { REQUEST_TYPE } from "../types";
import { useNavigate } from "react-router-dom";

const App = () => {
  const navigate = useNavigate();
  return (
    <div className="App">
      <button>Password Reset</button>
      <button>Create Temporary Access Password</button>
      <button
        onClick={() => {
          navigate("/redirectors/dismissUserRisk");
        }}
      >
        Dismiss User Risk
      </button>
    </div>
  );
};

export default App;
