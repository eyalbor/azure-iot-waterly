import React from 'react';

var paddingStyle ={
  padding: "3em 0em",
  marginTop: "25px"
}

const Footer = () => {
    return(
      <div className="ui inverted vertical footer segment" style={paddingStyle}>
        <div className="ui center aligned container">
        <img alt="logo" src="/logo192.png" className="ui centered mini image"/>
          <div className="ui horizontal inverted small divided link list">
            <a className="item" href="/">Home Page</a>
            <a className="item" href="/">TAU IOT</a>
            <a className="item" href="/">More</a>
          </div>
        </div>
      </div>
    )
  }

export default Footer;