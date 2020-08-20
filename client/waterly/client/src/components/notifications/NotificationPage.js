//https://devexpress.github.io/devextreme-reactive/react/chart/

import React from 'react'
import { connect } from 'react-redux'
import NotificationsTable from './NotificationsTable';

class NotificationPage extends React.Component {
      
    renderList(){
        if(this.props.isSignedIn) {
            console.log(this.props.currentUserId)
            return (
               <NotificationsTable user_id={this.props.currentUserId}/>
            );
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    render() {
        return (
            <div className="ui container">
                {this.renderList()}
            </div>
        );
    }
}

//geting list of stream availble inside the component
const mapStateToProps = (state) => {
    //Object.values gets all the object inside and make it as array
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, null)(NotificationPage);