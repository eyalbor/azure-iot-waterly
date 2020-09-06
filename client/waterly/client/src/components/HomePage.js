import React from 'react';
import Footer from './Footer'
import './HomePage.css'

class HomePage extends React.Component {
    render() {
        return (
            <div>
                <div className="ui vertical masthead center aligned basic segment">
                    <div className="ui text container">
                        <h1 className="ui violet header">
                            <b>Waterly</b>
                        </h1>
                        <h3>Your Water Consumption Just Got Smarter.</h3>
                        {/* <div className="ui huge red google button">Sign In <i className="right arrow icon"></i></div> */}
                        <a href='http://kineret.org.il/miflasim/'><img  alt="kineret" src="/kineret.png" className="ui medium centered image"/></a>
                        
                        <br/>
                    </div>
                </div>
                <Footer className="footer"/>
            </div>
        );
    }
  }

export default HomePage;
