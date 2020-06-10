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

    render() {
        return (
        <div className="ui massive inverted menu">
            <Link to="/" className="header item">
                Waterly
            </Link>
            
            <div className="right menu">
                <Link className="item" onClick={this.showNotifications()}>
                {/* badgeContent={4} */}
                    <Badge color="error" anchorOrigin={{
                        vertical: 'bottom',
                        horizontal: 'right',
                    }}>
                        <NotificationsNoneIcon fontSize="large"/>
                    </Badge>
                </Link>
                <Link to="/devices/list" className="item">
                    Dashboard
                </Link>
                <Link to="/bill/show" className="item">
                    Pay Bill
                </Link>
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
        notifications: state.notifications
    }
}

export default connect(mapStateToProps, { fetchNotifications })(Header);