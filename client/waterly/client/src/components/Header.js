import React from 'react';
import { Link } from 'react-router-dom';
import { connect } from 'react-redux'
import { fetchNotifications } from '../actions'
import GoogleAuth from './GoogleAuth'
import Badge from '@material-ui/core/Badge';
import NotificationsNoneIcon from '@material-ui/icons/NotificationsNone';

class Header extends React.Component {

    componentDidMount(){
        this.props.fetchNotifications();
    }

    showNotifications(){
        console.log("showNotifications")
    }

    showIfSignin(){
        if(this.props.isSignedIn){
            console.log(this.props.notifications.length)
            return( <div className="right menu">
                    <button  className="ui secondary icon button" onClick={this.showNotifications}>
                        <Badge badgeContent={this.props.notifications.length} color="error" anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'right',
                        }}>
                            <NotificationsNoneIcon fontSize="large"/>
                        </Badge>
                    </button >
                    <Link to="/devices/list" className="item">
                        DashBoard
                    </Link>
                    <Link to="/bill/show" className="item">
                        Pay Bill
                    </Link>
                </div>
            );
        }
    }

    render() {
        return (
        <div className="ui massive inverted menu">
            <Link to="/" className="header item">
                Waterly
            </Link>
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

const mapStateToProps = (state, ownProps) => {
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        notifications: Object.values(state.notifications)
    }
}

export default connect(mapStateToProps, { fetchNotifications })(Header);