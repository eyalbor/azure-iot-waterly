import React from 'react';

var paddingStyle ={
  padding: "3em 0em",
  marginTop: "40px"
}

const Footer = () => {
    return(
      <div className="ui inverted masthead vertical footer segment" style={paddingStyle}>
        <div className="ui center aligned container">
        <img alt="logo" src="/logo200.png" className="ui centered mini image"/>
          <div className="ui big horizontal inverted divided link list">
            <a className="item" href="/">Home Page</a>
            <a className="item" href="https://shaharp3.wixsite.com/waterly">Info Site</a>
          </div>
        </div>
      </div>
    )
  }

export default Footer;