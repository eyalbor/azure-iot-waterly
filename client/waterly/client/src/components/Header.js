import React from 'react';
import { Link } from 'react-router-dom';
import { connect } from 'react-redux'
import { fetchNotifications } from '../actions'
import GoogleAuth from './GoogleAuth'
import { renderTime } from '../actions/timestamp'
import Badge from '@material-ui/core/Badge';
import NotificationsNoneIcon from '@material-ui/icons/NotificationsNone';

class Header extends React.Component {

    componentDidUpdate(prevProps, prevState) {
        // console.log(prevProps)
        if(prevProps.isSignedIn !== this.props.isSignedIn) {
            this.props.fetchNotifications();
        }  
    }

    showNotifications(){
        return this.props.notifications.map(alert => {
            return (
                <div className="item" key={alert.id}>
                    <div className="content">
                        <div className="ui violet header">
                            {alert.type}
                        </div> 
                    </div>
                    <div className="description">
                    <span className="ui sub header">{renderTime(alert.timestamp)} </span>
                        {alert.message}  
                    </div>
                    <br/>
                </div>
            )
        })
    }

    showIfSignin(){
        if(this.props.isSignedIn){
            //console.log(this.props.notifications.length)
            return( 
                <div className="right menu">
                    <button  className="ui secondary icon simple dropdown button">
                        <Badge badgeContent={this.props.notifications.length} color="error" anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'right',
                        }}>
                            <NotificationsNoneIcon fontSize="large"/>
                        </Badge>
                        <div className="menu">
                            <div className="header" style={{fontSize: "large"}}>
                                <i className="info circle icon"/>
                                Alerts
                            </div>
                            <div className="divider"></div>
                            <div className="ui list" style={{padding: "20px"}}>
                                {this.showNotifications()}
                                <div className="divider"></div>
                                <a href={'/notifications'}>See All Alert</a>
                            </div>
                        </div>
                    </button >
                    <Link to="/devices/list" className="item">
                        Dashboard
                    </Link>
                    <Link to="/bill/show" className="item">
                        My Bills
                    </Link>
                </div>
            );
        }
    }

    showUser(){
        {/* if(this.props.user != null && this.props.user.le != null){
            console.log(this.props.user)
            return <div style={{color: 'white'}}>Hello,</div>
        }
        else return <div>Hello, user</div> */}
    }

    render() {
        //console.log(this.state.auth.user.tt.Ad)
        return (
        <div className="ui massive inverted menu">
            <Link to="/" className="header item">
                Waterly
            </Link>
            {this.showUser()}
            <div className="right menu">
                {this.showIfSignin()}
                <div className="item">
                    <GoogleAuth/>
                </div>
            </div>
        </div>
        )
    }
};

const mapStateToProps = (state) => {
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        user: state.auth.user,
        notifications: Object.values(state.notifications).filter(o => o.status === true)
    }
}

export default connect(mapStateToProps, { fetchNotifications })(Header);