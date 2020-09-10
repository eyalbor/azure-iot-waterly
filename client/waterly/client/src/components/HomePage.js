import React from 'react';
import Footer from './Footer'
import './HomePage.css'

class HomePage extends React.Component {
    render() {
        return (
            <div>
                <div className="ui vertical masthead center aligned basic segment">
                    <div className="ui text container">
                        <img alt="logo" src="/logo200s.png" className="ui centered large image"/>
                        
                        <h1 className="ui violet header">
                            Your Water Consumption Just Got Smarter
                        </h1>
                        <br/>
                        {/* <div className="ui huge red google button">Sign In <i className="right arrow icon"></i></div> */}
                        <a href='http://kineret.org.il/miflasim/'>
                            <img title="see kineret water level" alt="see kineret water level" src="/kineret.png" className="ui small centered image"/>
                        </a>
                        <br/>
                    </div>
                </div>
                <Footer className="footer"/>
            </div>
        );
    }
  }

export default HomePage;
