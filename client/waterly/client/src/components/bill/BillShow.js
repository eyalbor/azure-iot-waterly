import React from 'react'
import { connect } from 'react-redux'
import { fetchBills, payForBill } from '../../actions'
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from 'reactstrap';
import Paper from '@material-ui/core/Paper';
import {
    Chart,
    ArgumentAxis,
    ValueAxis,
    BarSeries,
    Title,
    Legend,
  } from '@devexpress/dx-react-chart-material-ui';
import { Stack, Animation } from '@devexpress/dx-react-chart';

import { withStyles } from '@material-ui/core/styles';

const legendStyles = () => ({
    root: {
        display: 'flex',
        margin: 'auto',
        flexDirection: 'row'
    },
});

const legendRootBase = ({ classes, ...restProps }) => (
    <Legend.Root {...restProps} className={classes.root} />
);
const Root = withStyles(legendStyles, { name: 'LegendRoot' })(legendRootBase);
const legendLabelStyles = () => ({
    label: {
        whiteSpace: 'nowrap',
}   ,
});
const legendLabelBase = ({ classes, ...restProps }) => (
    <Legend.Label className={classes.label} {...restProps} />
);
const Label = withStyles(legendLabelStyles, { name: 'LegendLabel' })(legendLabelBase);

class BillShow extends React.Component {

    constructor(props) {
        super(props)
        this.state = {
            modal: false,
            bill: null
        }
    }

    componentDidMount(){
        this.props.fetchBills();
    }

    toggle = (bill) => {
        console.log(bill)
        this.setState({modal: !this.state.modal, bill: bill})
    }

    showPayButton(bill){
        if(!bill.status){
            return(
                <div className="extra content">
                    <button className="ui primary button" onClick={()=>this.toggle(bill)}>Pay</button>
                </div>
            );
        }
    }

    pay = (bill) => {
        console.log('pay')
        console.log(bill)
        bill.status = true
        this.props.payForBill(bill);
        this.toggle()
    }

    renderCards(){
        console.log(this.props.bills)
        return this.props.bills.map(bill => {
            return (      
                <div className="ui card" key={bill.id} style={{padding: "20px", margin:"20px"}}>
                    <div className="content">
                        <div className="header">Month {bill.time.month}/{bill.time.year}</div>
                    </div>
                    <div className="content">
                        <h4 className="ui sub header">{bill.money.total_price} ILS</h4>
                        <div className="ui small feed">
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    Total flow for this week is: <a>{bill.total_flow} m³/h</a>
                                </div>
                                </div>
                            </div>
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    Water expenses: <a>{bill.money.water_expenses} ILS</a>
                                    <br/>
                                    Fixed expenses: <a>{bill.money.fixed_expenses} ILS</a>
                                    <br/>
                                    <b>Total price: {bill.money.water_expenses+bill.money.fixed_expenses} ILS</b>
                                </div>
                                </div>
                            </div>
                            <div className="event">
                                <div className="content">
                                <div className="summary">
                                    <Paper>
                                        <Chart data={[{month:`${bill.time.month}`,
                                         user: bill.money.water_expenses+bill.money.fixed_expenses ,
                                         avg: bill.avg}]}>
                                            <ArgumentAxis/>
                                            <ValueAxis/>
                                            <BarSeries name="User" valueField="user" argumentField="month" color="#ffd700"/>
                                            <BarSeries name="Avg" valueField="avg" argumentField="month" color="#c0c0c0"/>
                                            <Animation />
                                            <Legend position="bottom" rootComponent={Root} labelComponent={Label} />
                                            <Title text="Monthly Usage Of Water (m³/h)" />
                                            <Stack />
                                        </Chart>
                                    </Paper>
                                </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    {this.showPayButton(bill)}
                </div>
            )
        })
    }

    render(){
        return (
            <>
                <div className="ui container">
                    <div className="ui special cards">
                        {this.renderCards()}
                    </div>
                </div>

                <Modal isOpen={this.state.modal} toggle={this.toggle}>
                    <ModalHeader toggle={this.toggle}>Paying Method</ModalHeader>
                    <ModalBody>
                        The customer will choose how to pay the bill.<br/>
                        Amount: {this.state.modal? this.state.bill.money.water_expenses+this.state.bill.money.fixed_expenses:''}
                    </ModalBody>
                    <ModalFooter>
                        <Button color="primary" onClick={() => this.pay(this.state.bill)}>Confirm</Button>{' '}
                        <Button color="secondary" onClick={this.toggle}>Cancel</Button>
                    </ModalFooter>
                </Modal>
            </>
        )
    }
  
};

const mapStateToProps = (state) => {
    return {
        bills: Object.values(state.bills),
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, {fetchBills, payForBill})(BillShow);