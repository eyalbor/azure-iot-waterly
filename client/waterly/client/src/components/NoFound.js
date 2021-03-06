import React from 'react';
import { Link } from 'react-router-dom';
// import PageNotFound from '../assets/images/PageNotFound';
class NoFound extends React.Component{
    render(){
        return <div className="ui container">
            <h3 style={{textAlign:"center"}}>We are sorry but the page you are looking for does not exist.</h3>
            <h2 style={{textAlign:"center"}}><Link to="/">Go to Home </Link></h2>
            <img alt="404" src="/404-page.png" className="ui centered image"/>
        </div>
    }
}
export default NoFound;